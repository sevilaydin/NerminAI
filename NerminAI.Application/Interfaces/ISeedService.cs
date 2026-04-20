using NerminAI.Application.DTOs;

namespace NerminAI.Application.Interfaces
{
    public interface ISeedService
    {
        Task SeedDocumentAsync(SeedRequest request);
        Task SeedDefaultDataAsync();
        Task<int> SeedChatExportAsync(string? exportDirectory = null, string? keywordRegex = null, bool includeAll = false);
    }
}
