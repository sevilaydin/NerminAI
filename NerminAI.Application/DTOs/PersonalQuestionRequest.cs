namespace NerminAI.Application.DTOs
{
    public class ConversationMessage
    {
        public string Role { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }

    public class PersonalQuestionRequest
    {
        public string Question { get; set; } = string.Empty;
        public int TopK { get; set; } = 7;
        public double MinSimilarity { get; set; } = 0.25;
        public bool IncludeSources { get; set; } = true;
        public bool NoLLMFallback { get; set; } = false;
        public List<ConversationMessage> ConversationHistory { get; set; } = new();
        public List<string> LastSuggestions { get; set; } = new();
    }
}
