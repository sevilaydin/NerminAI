using NerminAI.Application.DTOs;

namespace NerminAI.Application.Interfaces
{
    public interface IPersonalQuestionService
    {
        Task<PersonalQuestionResponse> AskAsync(PersonalQuestionRequest request);
    }
}
