using System.Diagnostics;
using NerminAI.Application.DTOs;
using NerminAI.Application.Interfaces;
using NerminAI.Domain.Interfaces;

namespace NerminAI.Application.Services
{
    public class AskService : IAskService
    {
        private readonly IVectorRepository _vectorRepository;
        private readonly IEmbeddingService _embeddingService;
        private readonly ILLMService _llmService;

        public AskService(
            IVectorRepository vectorRepository,
            IEmbeddingService embeddingService,
            ILLMService llmService)
        {
            _vectorRepository = vectorRepository;
            _embeddingService = embeddingService;
            _llmService = llmService;
        }

        public async Task<AskResponse> AskAsync(AskRequest request)
        {
            var sw = Stopwatch.StartNew();

            // 1. Generate embedding for the question
            var queryEmbedding = await _embeddingService.GenerateEmbeddingAsync(request.Question);

            // 2. Search similar chunks
            var chunks = (await _vectorRepository.SearchSimilarChunksAsync(
                queryEmbedding, request.TopK, request.MinSimilarity)).ToList();

            // 3. Generate answer
            string answer;
            string mode;

            if (_llmService.IsAvailable)
            {
                if (chunks.Any())
                {
                    var contextTexts = chunks.Select(c => c.Content);
                    answer = await _llmService.GenerateAnswerAsync(request.Question, contextTexts);
                    mode = "LLM";
                }
                else
                {
                    // No matching chunks - let LLM answer freely
                    answer = await _llmService.GenerateAnswerAsync(request.Question, Enumerable.Empty<string>());
                    mode = "LLM-NoContext";
                }
            }
            else
            {
                answer = "Yanıt üretim servisi şu anda kullanılamıyor. Lütfen daha sonra tekrar deneyin.";
                mode = "Unavailable";
            }

            sw.Stop();

            return new AskResponse
            {
                Answer = answer,
                ConfidenceScore = chunks.Any() ? 0.85 : 0,
                GenerationMode = mode,
                Sources = new List<SourceDto>(),
                LatencyMs = sw.ElapsedMilliseconds
            };
        }
    }
}
