using NerminAI.Application.DTOs;

namespace NerminAI.Application.Interfaces
{
    public interface IProfileService
    {
        Task<ProfileDto?> GetActiveProfileAsync();
        Task UpsertProfileAsync(UpsertProfileRequest request);
        Task<PersonalitySummaryResponse> GetPersonalitySummaryAsync();
    }
}
