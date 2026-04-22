namespace NerminAI.Domain.Interfaces
{
    public interface ILLMService
    {
        Task<string> GenerateAnswerAsync(string question, IEnumerable<string> contextChunks);
        Task<string> GenerateEstimateAsync(string projectDescription, IEnumerable<string> skillContext);

        /// <summary>
        /// Generates a personal, persona-aware answer using a custom system prompt built from PersonProfile.
        /// When noLLMFallback is true, returns raw retrieved context instead of calling the LLM.
        /// </summary>
        Task<string> GeneratePersonalAnswerAsync(
            string question,
            string personaSystemPrompt,
            IEnumerable<string> retrievedChunks,
            bool noLLMFallback = false,
            IEnumerable<(string Role, string Content)>? conversationHistory = null,
            IEnumerable<string>? lastSuggestions = null);

        bool IsAvailable { get; }
    }
}
