using System.Diagnostics;
using System.Text;
using NerminAI.Application.DTOs;
using NerminAI.Application.Interfaces;
using NerminAI.Domain.Enums;
using NerminAI.Domain.Interfaces;

namespace NerminAI.Application.Services
{
    public class RecommendService : IRecommendService
    {
        private readonly IInterestRepository _interestRepository;
        private readonly IPreferenceRepository _preferenceRepository;
        private readonly IPersonProfileRepository _profileRepository;
        private readonly IEmbeddingService _embeddingService;
        private readonly IVectorRepository _vectorRepository;
        private readonly ILLMService _llmService;

        public RecommendService(
            IInterestRepository interestRepository,
            IPreferenceRepository preferenceRepository,
            IPersonProfileRepository profileRepository,
            IEmbeddingService embeddingService,
            IVectorRepository vectorRepository,
            ILLMService llmService)
        {
            _interestRepository = interestRepository;
            _preferenceRepository = preferenceRepository;
            _profileRepository = profileRepository;
            _embeddingService = embeddingService;
            _vectorRepository = vectorRepository;
            _llmService = llmService;
        }

        public async Task<RecommendationResponse> RecommendAsync(RecommendationRequest request)
        {
            var sw = Stopwatch.StartNew();

            var profile = await _profileRepository.GetActiveProfileAsync();
            var passionateInterests = (await _interestRepository.GetByLevelAsync(InterestLevel.Passionate)).ToList();
            var interestedItems = (await _interestRepository.GetByLevelAsync(InterestLevel.Interested)).ToList();
            var allInterests = passionateInterests.Concat(interestedItems).ToList();
            var strongPreferences = (await _preferenceRepository.GetByCategoryAsync("leisure"))
                .Concat(await _preferenceRepository.GetByCategoryAsync("hobiler"))
                .ToList();

            // Semantic context from vector search
            var semanticContext = new List<string>();
            var searchQuery = request.Context ?? string.Join(", ", allInterests.Take(5).Select(i => i.Name));
            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                var embedding = await _embeddingService.GenerateEmbeddingAsync(searchQuery);
                var chunks = await _vectorRepository.SearchSimilarChunksAsync(embedding, 5, 0.2);
                semanticContext = chunks.Select(c => c.Content).ToList();
            }

            string narrative;
            List<string> suggestions;
            string mode;

            if (_llmService.IsAvailable)
            {
                var sb = new StringBuilder();
                if (allInterests.Any())
                    sb.AppendLine($"İlgi alanları: {string.Join(", ", allInterests.Select(i => $"{i.Name} ({i.Level})"))}");
                if (strongPreferences.Any())
                    sb.AppendLine($"Tercihler: {string.Join(", ", strongPreferences.Select(p => p.Item))}");
                if (request.Context is not null)
                    sb.AppendLine($"Bağlam: {request.Context}");

                var allContext = semanticContext.Concat(new[] { sb.ToString() }).ToList();

                var name = profile?.FullName ?? "Bu kişi";
                var systemPrompt = $@"Sen {name}'in kişisel AI asistanısın.
Kişinin ilgi alanlarına ve tercihlerine göre samimi, pratik aktivite önerileri sun.
Sadece sağlanan bağlamdaki bilgileri kullan. En fazla {request.MaxSuggestions} öneri ver.
Her öneriyi neden uygun olduğunu kısaca açıkla. Sıcak ve doğal bir ton kullan.";

                narrative = await _llmService.GeneratePersonalAnswerAsync(
                    request.Context ?? "Bu kişi için aktivite önerisi ver.",
                    systemPrompt,
                    allContext);

                // Parse suggestions from narrative (simple extraction)
                suggestions = ExtractSuggestions(narrative, request.MaxSuggestions);
                mode = "PersonalLLM";
            }
            else
            {
                // NoLLM template fallback
                suggestions = allInterests
                    .Take(request.MaxSuggestions)
                    .Select(i => $"{i.Name} ile ilgili aktiviteler")
                    .ToList();

                narrative = suggestions.Any()
                    ? $"İlgi alanlarına göre şunlar önerilebilir: {string.Join(", ", suggestions)}."
                    : "Öneri yapabilmek için ilgi alanı bilgisi bulunamadı.";

                mode = "NoLLM-Template";
            }

            sw.Stop();

            return new RecommendationResponse
            {
                Suggestions = suggestions,
                Narrative = narrative,
                GenerationMode = mode,
                LatencyMs = sw.ElapsedMilliseconds
            };
        }

        private static List<string> ExtractSuggestions(string narrative, int max)
        {
            // Extract bullet points or numbered items from LLM narrative
            var lines = narrative.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            var suggestions = lines
                .Where(l => l.TrimStart().StartsWith("•") || l.TrimStart().StartsWith("-") ||
                            (l.Length > 2 && char.IsDigit(l.TrimStart()[0])))
                .Select(l => l.TrimStart('•', '-', ' ', '\t').Trim())
                .Where(l => l.Length > 5)
                .Take(max)
                .ToList();

            return suggestions.Any() ? suggestions : new List<string> { narrative };
        }
    }
}
