using System.Diagnostics;
using NerminAI.Application.DTOs;
using NerminAI.Application.Interfaces;
using NerminAI.Domain.Entities;
using NerminAI.Domain.Interfaces;

namespace NerminAI.Application.Services
{
    public class PersonalQuestionService : IPersonalQuestionService
    {
        private readonly IVectorRepository _vectorRepository;
        private readonly IEmbeddingService _embeddingService;
        private readonly ILLMService _llmService;
        private readonly IPersonProfileRepository _profileRepository;

        public PersonalQuestionService(
            IVectorRepository vectorRepository,
            IEmbeddingService embeddingService,
            ILLMService llmService,
            IPersonProfileRepository profileRepository)
        {
            _vectorRepository = vectorRepository;
            _embeddingService = embeddingService;
            _llmService = llmService;
            _profileRepository = profileRepository;
        }

        public async Task<PersonalQuestionResponse> AskAsync(PersonalQuestionRequest request)
        {
            var sw = Stopwatch.StartNew();

            // 1. Load active persona to build system prompt
            var profile = await _profileRepository.GetActiveProfileAsync();
            var systemPrompt = BuildPersonaSystemPrompt(profile);

            // 2. Embed the question — kısa/belirsiz sorgularda önceki user mesajını ekle
            var searchQuery = BuildSearchQuery(request.Question, request.ConversationHistory);
            var queryEmbedding = await _embeddingService.GenerateEmbeddingAsync(searchQuery);

            // 3. Vector search
            var chunks = (await _vectorRepository.SearchSimilarChunksAsync(
                queryEmbedding, request.TopK, request.MinSimilarity)).ToList();

            // 3b. Keyword fallback — boost retrieval with direct keyword search
            var keywords = ExtractKeywords(searchQuery);
            if (keywords.Any())
            {
                var keywordChunks = (await _vectorRepository.SearchByKeywordsAsync(keywords, topK: 3)).ToList();
                foreach (var kc in keywordChunks)
                {
                    if (!chunks.Any(c => c.Content == kc.Content))
                        chunks.Add(kc);
                }
            }

            // 4. Build sources list from chunk metadata
            var sources = request.IncludeSources
                ? chunks.Select(c => BuildSourceDto(c)).ToList()
                : new List<PersonalSourceDto>();

            // 5. Generate answer
            string answer;
            string mode;

            if (_llmService.IsAvailable)
            {
                var contextTexts = chunks.Select(c => c.Content);
                var history = request.ConversationHistory
                    .Select(m => (m.Role, m.Content));
                answer = await _llmService.GeneratePersonalAnswerAsync(
                    request.Question, systemPrompt, contextTexts, request.NoLLMFallback, history, request.LastSuggestions);

                mode = request.NoLLMFallback
                    ? "NoLLM-Template"
                    : chunks.Any() ? "PersonalLLM" : "PersonalLLM-NoContext";
            }
            else
            {
                answer = BuildNoLLMFallback(request.Question, profile, chunks);
                mode = "NoLLM-Template";
            }

            sw.Stop();

            return new PersonalQuestionResponse
            {
                Answer = answer,
                ConfidenceScore = chunks.Any() ? 0.88 : 0.1,
                GenerationMode = mode,
                Sources = sources,
                LatencyMs = sw.ElapsedMilliseconds
            };
        }

        private static string BuildSearchQuery(string question, List<ConversationMessage> history)
        {
            // Kısa/belirsiz sorgular için önceki konuşmadan bağlam ekle
            var words = question.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (words.Length <= 3 && history.Count > 0)
            {
                // Son birkaç mesajdan konuyu çek
                var recentContext = string.Join(" ", history
                    .TakeLast(4)
                    .Select(m => m.Content)
                    .Where(c => c.Length > 5));
                return $"{recentContext} {question}".Trim();
            }
            return question;
        }

        private static string BuildPersonaSystemPrompt(PersonProfile? profile)
        {
            if (profile is null)
            {
                return @"Sen kişisel bir AI asistanısın.
KURAL: Yalnızca sağlanan bağlam bilgisine dayanarak cevap ver.
Eğer bağlam yetersizse, dürüstçe 'Bu konuda yeterli bilgim bulunmuyor.' de.
Sorunun dilinde yanıt ver.";
            }

            var traits = profile.PersonalityTraits.Any()
                ? string.Join(", ", profile.PersonalityTraits)
                : "bilinmiyor";

            return $@"Sen {profile.FullName} adlı kişinin kişisel AI asistanısın.
Bu kişi hakkında konuşurken {profile.SpeakingStyle} kullan.
Ton: {profile.ToneStyle}
Kişilik özellikleri: {traits}

KATKI KURALLARI (ÇOK ÖNEMLİ):
1. Yalnızca sağlanan bağlam bilgisinde var olan bilgileri kullan.
2. Hiçbir şeyi uydurma (hallucinate etme) — bağlamda olmayan bilgileri ekme.
3. Eğer bir bilgiye sahip değilsen, açıkça 'Bu konuda yeterli bilgim bulunmuyor.' de.
4. Duygusal ve kişisel sorulara samimi, sıcak ve doğal cevap ver.
5. Robotik, listeleme bazlı cevaplardan kaçın — insan gibi konuş.
6. Varsayım yapman gerekiyorsa, bunu açıkça belirt.
7. Cevabı sorunun dilinde ver (Türkçe veya İngilizce).";
        }

        private static PersonalSourceDto BuildSourceDto(Chunk chunk)
        {
            var metadata = chunk.Document?.Metadata ?? "";
            var entityType = ExtractMetadataValue(metadata, "entityType") ?? chunk.Document?.Type.ToString() ?? "Unknown";
            var title = chunk.Document?.Title ?? "Bilinmeyen kaynak";
            var snippet = chunk.Content.Length > 200 ? chunk.Content[..200] + "..." : chunk.Content;

            return new PersonalSourceDto
            {
                EntityType = entityType,
                Title = title,
                Snippet = snippet,
                Similarity = 0 // similarity value is not carried through the current VectorRepository interface
            };
        }

        private static List<string> ExtractKeywords(string question)
        {
            var q = question.ToLowerInvariant();
            var keywords = new List<string>();

            var topicMap = new Dictionary<string, string[]>
            {
                ["aile"]      = ["anne", "baba", "kardeş", "aile", "akraba"],
                ["eş"]        = ["eşim", "eşimle", "kocam", "evlilik", "evliyiz", "eşin", "eşinin", "samir"],
                ["kedi"]      = ["kedi", "kedim", "kediler", "british", "siamese", "tekir"],
                ["seyahat"]   = ["hollanda", "barcelona", "tiflis", "morocco", "tatil", "seyahat", "gez", "ülke", "şehir"],
                ["yemek"]     = ["yemek", "tarif", "kahve", "espresso", "tiramisu", "pasta", "yiyor", "sever"],
                ["alışveriş"] = ["dyson", "altın", "takı", "marka", "zara", "alışveriş"],
                ["hayal"]     = ["hayal", "vision", "hedef", "istek", "gelecek", "plan", "2025"],
                ["duygu"]     = ["sinir", "üzgün", "mutlu", "moral", "stres", "his", "duygu"],
                ["kişilik"]   = ["nasıl biri", "karakter", "kişilik", "özellik", "biri misin", "kim"],
                ["sağlık"]    = ["sağlık", "hastalık", "ameliyat", "ilaç", "doktor"],
                ["yaş"]       = ["yaş", "doğum", "burç", "astroloji", "kaç yaşında"],
            };

            foreach (var (_, words) in topicMap)
                foreach (var w in words)
                    if (q.Contains(w))
                        keywords.Add(w);

            return keywords.Distinct().Take(4).ToList();
        }

        private static string? ExtractMetadataValue(string metadata, string key)
        {
            if (string.IsNullOrEmpty(metadata)) return null;
            var parts = metadata.Split(';');
            foreach (var part in parts)
            {
                var kv = part.Split('=');
                if (kv.Length == 2 && kv[0].Trim().Equals(key, StringComparison.OrdinalIgnoreCase))
                    return kv[1].Trim();
            }
            return null;
        }

        private static string BuildNoLLMFallback(string question, PersonProfile? profile, List<Chunk> chunks)
        {
            var name = profile?.FullName ?? "Bu kişi";
            if (!chunks.Any())
                return $"{name} hakkında '{question}' sorusuna yanıt verebilecek bilgi bulunamadı.";

            var topContent = chunks.First().Content;
            return $"İlgili bilgi bulundu:\n\n{topContent}\n\n(Bu yanıt LLM kullanılmadan şablon ile oluşturulmuştur.)";
        }
    }
}
