using System.Text.Json;
using Microsoft.Extensions.Configuration;
using NerminAI.Domain.Entities;
using NerminAI.Domain.Interfaces;
using Npgsql;

namespace NerminAI.Infrastructure.Repositories
{
    /// <summary>
    /// IVectorRepository implementation that reads from the "Memories" table
    /// populated by the Python ingestion pipeline (data/load_db.py).
    /// Embeddings are stored as JSON text; cosine similarity is computed in-process.
    /// </summary>
    public class PersonalMemoryVectorRepository : IVectorRepository
    {
        private readonly string _connectionString;

        public PersonalMemoryVectorRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("DefaultConnection is missing.");
        }

        // ── IVectorRepository ───────────────────────────────────────────────

        // Topic keywords for boosting relevant chunks
        private static readonly Dictionary<string, string[]> TopicKeywords = new()
        {
            ["travel"]   = ["seyahat","tatil","gez","hollanda","barcelona","tiflis","morocco","fas","otel","ülke","şehir","uçuş"],
            ["food"]     = ["yemek","ye","içe","tarif","mutfak","kahve","espresso","börek","pasta","lezzet","restoran","hamburger"],
            ["shopping"] = ["al","satın","dyson","marka","ürün","altın","takı","mağaza","kıyafet"],
            ["pets"]     = ["kedi","köpek","hayvan","pati","mama","veteriner"],
            ["lifestyle"] = ["hayal","hedef","vision","manifest","plan","gelecek","2025","kendim"],
            ["emotion"]  = ["his","duygu","mutlu","üzgün","canım","moral","stres","kaygı"],
            ["family"]   = ["eşim","evlilik","aile","anne","baba","arkadaş","ilişki"],
        };

        public async Task<IEnumerable<Chunk>> SearchSimilarChunksAsync(
            float[] queryEmbedding, int topK = 8, double threshold = 0.3)
        {
            var rows = await FetchMemoryRowsAsync(sensitiveFilter: false);

            var allScored = rows
                .Select(r =>
                {
                    var sim = CosineSimilarity(queryEmbedding, r.Embedding);
                    var boost = TopicBoost(r);
                    return (r, rawSim: sim, score: sim + boost);
                })
                .OrderByDescending(x => x.score)
                .ToList();

            // Profile chunks always pass through; conversation chunks need rawSim >= 0.35
            bool IsProfileChunk(string chunkId) =>
                chunkId.StartsWith("profile_") || chunkId.StartsWith("pv2_") || chunkId.StartsWith("manual_");

            var filtered = allScored
                .Where(x => IsProfileChunk(x.r.ChunkId) || x.rawSim >= 0.35)
                .ToList();

            // If too few results, lower bar to 0.20 for non-profile chunks
            if (filtered.Count < 3)
            {
                filtered = allScored
                    .Where(x => IsProfileChunk(x.r.ChunkId) || x.rawSim >= 0.20)
                    .ToList();
            }

            var topResults = filtered.Take(topK).ToList();

            // Always include the best family chunk (eş, aile, samir)
            var bestFamily = allScored
                .FirstOrDefault(x => x.r.Topic is "family" && !topResults.Any(t => t.r.ChunkId == x.r.ChunkId));
            if (bestFamily.r != null)
                topResults.Add(bestFamily);

            return topResults.Select(x => MapToChunk(x.r, x.score));
        }

        private static double TopicBoost(MemoryRow row)
        {
            var lower = row.Content.ToLowerInvariant();

            // Profil chunk'ları en güvenilir — her zaman yüksek boost
            if (row.ChunkId.StartsWith("profile_") || row.ChunkId.StartsWith("manual_") || row.ChunkId.StartsWith("pv2_"))
                return 0.20;

            // Yüksek değerli kişisel chunk'lar
            if (lower.Contains("vision board") || lower.Contains("hayallerim") ||
                lower.Contains("hollanda") || lower.Contains("barcelona"))
                return 0.15;

            if (lower.Contains("eşim") || lower.Contains("eşimle") || lower.Contains("evliyiz") || lower.Contains("samir"))
                return 0.12;

            if (row.Topic is "lifestyle" or "travel" or "shopping" or "family")
                return 0.06;

            return 0;
        }

        /// <summary>
        /// Keyword-based search: directly queries DB for chunks containing any of the keywords.
        /// Used as a fallback when embedding similarity misses relevant personal content.
        /// </summary>
        public async Task<IEnumerable<Chunk>> SearchByKeywordsAsync(IEnumerable<string> keywords, int topK = 3)
        {
            var kws = keywords.Select(k => k.ToLowerInvariant()).ToList();
            if (!kws.Any()) return Enumerable.Empty<Chunk>();

            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            // Build WHERE clause with Npgsql named parameters
            var conditions = string.Join(" OR ", kws.Select((_, i) => $"LOWER(\"Content\") LIKE '%' || @p{i} || '%'"));
            var sql = $@"SELECT ""ChunkId"", ""Content"", ""Topic"", ""Language"", ""IsSensitive"", ""ConfidenceTier""
                         FROM ""Memories""
                         WHERE ""IsSensitive"" = FALSE AND ({conditions})
                         LIMIT {topK * 3}";

            await using var cmd = new NpgsqlCommand(sql, conn);
            for (int i = 0; i < kws.Count; i++)
                cmd.Parameters.AddWithValue($"p{i}", kws[i]);

            var rows = new List<MemoryRow>();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                rows.Add(new MemoryRow
                {
                    ChunkId = reader.GetString(0),
                    Content = reader.GetString(1),
                    Topic = reader.IsDBNull(2) ? "misc" : reader.GetString(2),
                    Language = reader.IsDBNull(3) ? "tr" : reader.GetString(3),
                    IsSensitive = reader.GetBoolean(4),
                    ConfidenceTier = reader.GetInt32(5),
                    Embedding = Array.Empty<float>(),
                });
            }

            // Pick most content-rich rows
            return rows
                .OrderByDescending(r => r.Content.Length)
                .Take(topK)
                .Select(r => MapToChunk(r, 0.5));
        }

        public Task<Embedding> AddEmbeddingAsync(Guid chunkId, float[] vector, string model)
            => throw new NotSupportedException(
                "PersonalMemoryVectorRepository is read-only. Use the Python ingest pipeline.");

        public Task<bool> HasEmbeddingAsync(Guid chunkId)
            => throw new NotSupportedException(
                "PersonalMemoryVectorRepository is read-only. Use the Python ingest pipeline.");

        // ── Internal helpers ────────────────────────────────────────────────

        private async Task<List<MemoryRow>> FetchMemoryRowsAsync(bool sensitiveFilter)
        {
            var rows = new List<MemoryRow>();

            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            var sql = sensitiveFilter
                ? @"SELECT ""ChunkId"", ""Content"", ""Topic"", ""Language"",
                           ""IsSensitive"", ""ConfidenceTier"", ""Embedding""
                    FROM ""Memories""
                    WHERE ""IsSensitive"" = FALSE AND ""Embedding"" IS NOT NULL"
                : @"SELECT ""ChunkId"", ""Content"", ""Topic"", ""Language"",
                           ""IsSensitive"", ""ConfidenceTier"", ""Embedding""
                    FROM ""Memories""
                    WHERE ""IsSensitive"" = FALSE AND ""Embedding"" IS NOT NULL";

            await using var cmd = new NpgsqlCommand(sql, conn);
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var embJson = reader.IsDBNull(6) ? null : reader.GetString(6);
                if (embJson is null) continue;

                float[]? emb;
                try { emb = JsonSerializer.Deserialize<float[]>(embJson); }
                catch { continue; }
                if (emb is null) continue;

                rows.Add(new MemoryRow
                {
                    ChunkId = reader.GetString(0),
                    Content = reader.GetString(1),
                    Topic = reader.IsDBNull(2) ? "misc" : reader.GetString(2),
                    Language = reader.IsDBNull(3) ? "tr" : reader.GetString(3),
                    IsSensitive = reader.GetBoolean(4),
                    ConfidenceTier = reader.GetInt32(5),
                    Embedding = emb,
                });
            }

            return rows;
        }

        private static Chunk MapToChunk(MemoryRow row, double similarity)
        {
            // We reuse Chunk entity. Store topic/confidence in a synthetic Document.
            var doc = new Document
            {
                Id = Guid.NewGuid(),
                Title = $"[{row.Topic}] personal memory",
                Content = row.Content,
                Type = Domain.Enums.DocumentType.PersonalMemory,
                Metadata = $"topic={row.Topic};lang={row.Language};confidence={row.ConfidenceTier};similarity={similarity:F3}",
            };

            return new Chunk
            {
                Id = Guid.NewGuid(),
                DocumentId = doc.Id,
                Document = doc,
                Content = row.Content,
                ChunkIndex = 0,
                TokenCount = row.Content.Length / 4,
            };
        }

        private static double CosineSimilarity(float[] a, float[] b)
        {
            if (a.Length != b.Length) return 0;
            double dot = 0, magA = 0, magB = 0;
            for (int i = 0; i < a.Length; i++)
            {
                dot += a[i] * b[i];
                magA += a[i] * a[i];
                magB += b[i] * b[i];
            }
            var mag = Math.Sqrt(magA) * Math.Sqrt(magB);
            return mag == 0 ? 0 : dot / mag;
        }

        // ── DTO ─────────────────────────────────────────────────────────────

        private sealed class MemoryRow
        {
            public string ChunkId { get; set; } = "";
            public string Content { get; set; } = "";
            public string Topic { get; set; } = "misc";
            public string Language { get; set; } = "tr";
            public bool IsSensitive { get; set; }
            public int ConfidenceTier { get; set; }
            public float[] Embedding { get; set; } = Array.Empty<float>();
        }
    }
}
