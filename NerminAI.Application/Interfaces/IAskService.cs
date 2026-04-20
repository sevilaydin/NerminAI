using NerminAI.Application.DTOs;

namespace NerminAI.Application.Interfaces
{
    public interface IAskService
    {
        Task<AskResponse> AskAsync(AskRequest request);
    }
}
