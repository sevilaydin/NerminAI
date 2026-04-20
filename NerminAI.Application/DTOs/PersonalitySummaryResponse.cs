namespace NerminAI.Application.DTOs
{
    public class PersonalitySummaryResponse
    {
        public string FullName { get; set; } = string.Empty;
        public string[] Traits { get; set; } = Array.Empty<string>();
        public string ToneStyle { get; set; } = string.Empty;
        public Dictionary<string, int> InterestBreakdown { get; set; } = new();
        public Dictionary<string, int> RelationshipBreakdown { get; set; } = new();
        public string LLMNarrative { get; set; } = string.Empty;
        public long LatencyMs { get; set; }
    }
}
