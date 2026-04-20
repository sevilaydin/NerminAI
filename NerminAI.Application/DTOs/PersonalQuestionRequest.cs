namespace NerminAI.Application.DTOs
{
    public class PersonalQuestionRequest
    {
        public string Question { get; set; } = string.Empty;
        public int TopK { get; set; } = 7;
        public double MinSimilarity { get; set; } = 0.25;
        public bool IncludeSources { get; set; } = true;

        /// <summary>
        /// When true, skips the LLM and returns raw retrieved chunks as plain text.
        /// Useful for privacy-sensitive or cost-sensitive scenarios.
        /// </summary>
        public bool NoLLMFallback { get; set; } = false;
    }
}
