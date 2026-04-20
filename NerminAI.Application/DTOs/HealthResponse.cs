namespace NerminAI.Application.DTOs
{
    public class HealthResponse
    {
        public string Status { get; set; } = "healthy";
        public int DocumentCount { get; set; }
        public int ChunkCount { get; set; }
        public bool LLMAvailable { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
