namespace NerminAI.Application.DTOs
{
    public class RecommendationRequest
    {
        /// <summary>
        /// Optional context for recommendations.
        /// Example: "I want to relax this weekend" or "I need a new hobby"
        /// </summary>
        public string? Context { get; set; }

        public int MaxSuggestions { get; set; } = 5;
    }
}
