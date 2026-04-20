using NerminAI.Application.DTOs;

namespace NerminAI.Application.Interfaces
{
    public interface IEstimateService
    {
        Task<EstimateResponse> EstimateAsync(EstimateRequest request);
    }
}
