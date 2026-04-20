using Microsoft.EntityFrameworkCore;
using NerminAI.Domain.Entities;
using NerminAI.Domain.Interfaces;
using NerminAI.Infrastructure.Data;

namespace NerminAI.Infrastructure.Repositories
{
    public class VectorRepository : IVectorRepository
    {
        private readonly ApplicationDbContext _context;

        public VectorRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Chunk>> SearchSimilarChunksAsync(
            float[] queryEmbedding, int topK = 5, double threshold = 0.7)
        {
            var embeddings = await _context.Embeddings
                .Include(e => e.Chunk)
                    .ThenInclude(c => c.Document)
                .ToListAsync();

            var results = embeddings
                .Select(e => new
                {
                    e.Chunk,
                    Similarity = CosineSimilarity(queryEmbedding, e.Vector)
                })
                .Where(r => r.Similarity >= threshold)
                .OrderByDescending(r => r.Similarity)
                .Take(topK)
                .Select(r => r.Chunk)
                .ToList();

            return results;
        }

        public Task<IEnumerable<Chunk>> SearchByKeywordsAsync(IEnumerable<string> keywords, int topK = 3)
            => Task.FromResult(Enumerable.Empty<Chunk>());

        public async Task<Embedding> AddEmbeddingAsync(Guid chunkId, float[] vector, string model)
        {
            var embedding = new Embedding
            {
                ChunkId = chunkId,
                Vector = vector,
                Model = model
            };

            _context.Embeddings.Add(embedding);
            await _context.SaveChangesAsync();
            return embedding;
        }

        public async Task<bool> HasEmbeddingAsync(Guid chunkId)
            => await _context.Embeddings.AnyAsync(e => e.ChunkId == chunkId);

        private static double CosineSimilarity(float[] a, float[] b)
        {
            if (a.Length != b.Length) return 0;

            double dotProduct = 0, magnitudeA = 0, magnitudeB = 0;
            for (int i = 0; i < a.Length; i++)
            {
                dotProduct += a[i] * b[i];
                magnitudeA += a[i] * a[i];
                magnitudeB += b[i] * b[i];
            }

            var magnitude = Math.Sqrt(magnitudeA) * Math.Sqrt(magnitudeB);
            return magnitude == 0 ? 0 : dotProduct / magnitude;
        }
    }
}
