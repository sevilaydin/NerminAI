using NerminAI.Application.DTOs;
using NerminAI.Application.Interfaces;
using NerminAI.Domain.Interfaces;

namespace NerminAI.Application.Services
{
    public class HealthService : IHealthService
    {
        private readonly IDocumentRepository _documentRepository;
        private readonly IChunkRepository _chunkRepository;
        private readonly ILLMService _llmService;

        public HealthService(
            IDocumentRepository documentRepository,
            IChunkRepository chunkRepository,
            ILLMService llmService)
        {
            _documentRepository = documentRepository;
            _chunkRepository = chunkRepository;
            _llmService = llmService;
        }

        public async Task<HealthResponse> GetHealthAsync()
        {
            return new HealthResponse
            {
                Status = "healthy",
                DocumentCount = await _documentRepository.GetDocumentCountAsync(),
                ChunkCount = await _chunkRepository.GetChunkCountAsync(),
                LLMAvailable = _llmService.IsAvailable
            };
        }
    }
}
