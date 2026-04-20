namespace NerminAI.Application.DTOs
{
    public class MemoryReflectionResponse
    {
        public string Reflection { get; set; } = string.Empty;
        public int MemoryCount { get; set; }
        public string GenerationMode { get; set; } = "PersonalLLM";
        public long LatencyMs { get; set; }
    }
}
