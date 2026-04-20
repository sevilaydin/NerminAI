using NerminAI.Application.DTOs;

namespace NerminAI.Application.Interfaces
{
    public interface IHealthService
    {
        Task<HealthResponse> GetHealthAsync();
    }
}
