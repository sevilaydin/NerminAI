namespace NerminAI.Application.DTOs
{
    public class RecommendationResponse
    {
        public List<string> Suggestions { get; set; } = new();
        public string Narrative { get; set; } = string.Empty;
        public string GenerationMode { get; set; } = "PersonalLLM";
        public long LatencyMs { get; set; }
    }
}
