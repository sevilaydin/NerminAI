namespace NerminAI.Application.DTOs
{
    public class PersonalQuestionResponse
    {
        public string Answer { get; set; } = string.Empty;
        public double ConfidenceScore { get; set; }
        public string GenerationMode { get; set; } = "PersonalLLM";
        public List<PersonalSourceDto> Sources { get; set; } = new();
        public long LatencyMs { get; set; }
    }

    public class PersonalSourceDto
    {
        public string EntityType { get; set; } = string.Empty; // Memory, Preference, Interest, etc.
        public string Title { get; set; } = string.Empty;
        public string Snippet { get; set; } = string.Empty;
        public double Similarity { get; set; }
    }
}
