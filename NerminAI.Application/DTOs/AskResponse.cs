namespace NerminAI.Application.DTOs
{
    public class AskResponse
    {
        public string Answer { get; set; } = string.Empty;
        public double ConfidenceScore { get; set; }
        public string GenerationMode { get; set; } = "LLM";
        public List<SourceDto> Sources { get; set; } = new();
        public long LatencyMs { get; set; }
    }

    public class SourceDto
    {
        public string DocumentTitle { get; set; } = string.Empty;
        public string SourceType { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public double SimilarityScore { get; set; }
    }
}
