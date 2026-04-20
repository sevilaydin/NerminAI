using System.Diagnostics;
using System.Text;
using NerminAI.Application.DTOs;
using NerminAI.Application.Interfaces;
using NerminAI.Domain.Interfaces;

namespace NerminAI.Application.Services
{
    public class MemoryService : IMemoryService
    {
        private readonly ISeedPersonalDataService _seedService;
        private readonly IMemoryRepository _memoryRepository;
        private readonly IPersonProfileRepository _profileRepository;
        private readonly IEmbeddingService _embeddingService;
        private readonly IVectorRepository _vectorRepository;
        private readonly ILLMService _llmService;

        public MemoryService(
            ISeedPersonalDataService seedService,
            IMemoryRepository memoryRepository,
            IPersonProfileRepository profileRepository,
            IEmbeddingService embeddingService,
            IVectorRepository vectorRepository,
            ILLMService llmService)
        {
            _seedService = seedService;
            _memoryRepository = memoryRepository;
            _profileRepository = profileRepository;
            _embeddingService = embeddingService;
            _vectorRepository = vectorRepository;
            _llmService = llmService;
        }

        public async Task<AddMemoryResponse> AddMemoryAsync(AddMemoryRequest request)
        {
            // Delegate to SeedPersonalDataService for the dual-write (entity + shadow doc + embedding)
            var seedRequest = new SeedPersonalDataRequest
            {
                Memories = new List<MemorySeedDto>
                {
                    new MemorySeedDto
                    {
                        Title = request.Title,
                        Content = request.Content,
                        Date = request.MemoryDate?.ToString("yyyy-MM-dd"),
                        EmotionalTone = request.EmotionalTone,
                        Category = request.Category
                    }
                }
            };

            await _seedService.SeedAsync(seedRequest);

            // Retrieve the newly added memory
            var memories = await _memoryRepository.FindAsync(m => m.Title == request.Title);
            var added = memories.OrderByDescending(m => m.CreatedAt).FirstOrDefault();

            return new AddMemoryResponse
            {
                MemoryId = added?.Id ?? Guid.Empty,
                ShadowDocumentId = added?.ShadowDocumentId,
                Message = $"Anı '{request.Title}' başarıyla eklendi ve vektör veritabanına işlendi."
            };
        }

        public async Task<MemoryReflectionResponse> ReflectAsync(MemoryReflectionRequest request)
        {
            var sw = Stopwatch.StartNew();

            // 1. Structured query (date range, category, tone)
            var structuredMemories = new List<Domain.Entities.Memory>();

            if (request.FromDate.HasValue && request.ToDate.HasValue)
                structuredMemories.AddRange(await _memoryRepository.SearchByDateRangeAsync(request.FromDate.Value, request.ToDate.Value));
            else if (request.Category.HasValue)
                structuredMemories.AddRange(await _memoryRepository.SearchByCategoryAsync(request.Category.Value));
            else if (request.EmotionalTone.HasValue)
                structuredMemories.AddRange(await _memoryRepository.GetByEmotionalToneAsync(request.EmotionalTone.Value));
            else
                structuredMemories.AddRange(await _memoryRepository.GetAllAsync());

            // 2. Semantic search if free-form prompt given
            var semanticContexts = new List<string>();
            if (!string.IsNullOrWhiteSpace(request.FreeFormPrompt))
            {
                var queryEmbedding = await _embeddingService.GenerateEmbeddingAsync(request.FreeFormPrompt);
                var chunks = await _vectorRepository.SearchSimilarChunksAsync(queryEmbedding, 5, 0.2);
                semanticContexts = chunks.Select(c => c.Content).ToList();
            }

            // 3. Build combined context
            var memoryTexts = structuredMemories
                .Take(10)
                .Select(m => $"[{m.Category} - {m.EmotionalTone}] {m.Title} ({m.MemoryDate?.ToString("yyyy") ?? "?"}): {m.Content}");

            var allContext = memoryTexts.Concat(semanticContexts).ToList();

            // 4. Load profile for persona
            var profile = await _profileRepository.GetActiveProfileAsync();
            var prompt = request.FreeFormPrompt ?? "Bu anılar hakkında kısa ve samimi bir değerlendirme yap.";

            string reflection;
            string mode;

            if (_llmService.IsAvailable && allContext.Any())
            {
                var systemPrompt = profile is not null
                    ? $"Sen {profile.FullName}'in anılarını yansıtan kişisel bir AI asistanısın. Sıcak, dürüst ve duygusal zekaya sahip bir ton kullan."
                    : "Sen kişisel anıları yansıtan bir AI asistanısın. Sıcak bir ton kullan.";

                reflection = await _llmService.GeneratePersonalAnswerAsync(prompt, systemPrompt, allContext);
                mode = "PersonalLLM";
            }
            else if (allContext.Any())
            {
                // NoLLM template fallback
                var sb = new StringBuilder();
                sb.AppendLine($"Bulunan {structuredMemories.Count} anı:\n");
                foreach (var m in structuredMemories.Take(5))
                    sb.AppendLine($"• [{m.Category}] {m.Title} ({m.MemoryDate?.ToString("yyyy-MM-dd") ?? "tarih bilinmiyor"})");
                reflection = sb.ToString();
                mode = "NoLLM-Template";
            }
            else
            {
                reflection = "Bu kriterlere uygun anı bulunamadı.";
                mode = "NoLLM-Template";
            }

            sw.Stop();

            return new MemoryReflectionResponse
            {
                Reflection = reflection,
                MemoryCount = structuredMemories.Count,
                GenerationMode = mode,
                LatencyMs = sw.ElapsedMilliseconds
            };
        }
    }
}
