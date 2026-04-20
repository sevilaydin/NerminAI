using NerminAI.Application.DTOs;

namespace NerminAI.Application.Interfaces
{
    public interface IRecommendService
    {
        Task<RecommendationResponse> RecommendAsync(RecommendationRequest request);
    }
}
