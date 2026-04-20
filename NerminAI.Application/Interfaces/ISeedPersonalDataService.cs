using NerminAI.Application.DTOs;

namespace NerminAI.Application.Interfaces
{
    public interface ISeedPersonalDataService
    {
        Task SeedAsync(SeedPersonalDataRequest request);
    }
}
