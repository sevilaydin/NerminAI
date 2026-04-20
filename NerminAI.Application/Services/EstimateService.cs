using System.Diagnostics;
using NerminAI.Application.DTOs;
using NerminAI.Application.Interfaces;
using NerminAI.Domain.Enums;
using NerminAI.Domain.Interfaces;

namespace NerminAI.Application.Services
{
    public class EstimateService : IEstimateService
    {
        private readonly IDocumentRepository _documentRepository;
        private readonly ILLMService _llmService;

        public EstimateService(IDocumentRepository documentRepository, ILLMService llmService)
        {
            _documentRepository = documentRepository;
            _llmService = llmService;
        }

        public async Task<EstimateResponse> EstimateAsync(EstimateRequest request)
        {
            var sw = Stopwatch.StartNew();

            // Get skill-related documents for context
            var skillDocs = await _documentRepository.GetByTypeAsync(DocumentType.Skill);
            var expDocs = await _documentRepository.GetByTypeAsync(DocumentType.Experience);

            var contextDocs = skillDocs.Concat(expDocs).Select(d => d.Content).ToList();

            string estimate;
            if (_llmService.IsAvailable && contextDocs.Any())
            {
                estimate = await _llmService.GenerateEstimateAsync(request.ProjectDescription, contextDocs);
            }
            else
            {
                estimate = BuildTemplateEstimate(request.ProjectDescription);
            }

            sw.Stop();

            return new EstimateResponse
            {
                Estimate = estimate,
                LatencyMs = sw.ElapsedMilliseconds
            };
        }

        private static string BuildTemplateEstimate(string projectDescription)
        {
            return $@"## Project Effort Estimation

**Project:** {projectDescription}

### Estimated Breakdown:
- **Planning & Design:** 2-3 days
- **Backend Development:** 5-10 days
- **Frontend Development:** 3-5 days
- **Testing & QA:** 2-3 days
- **Deployment & DevOps:** 1-2 days

### Total Estimate: 13-23 person-days

*Note: This is a template-based estimate. Enable LLM for a more detailed, context-aware estimation.*";
        }
    }
}
