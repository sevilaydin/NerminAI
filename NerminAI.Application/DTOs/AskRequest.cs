namespace NerminAI.Application.DTOs
{
    public class AskRequest
    {
        public string Question { get; set; } = string.Empty;
        public int TopK { get; set; } = 5;
        public double MinSimilarity { get; set; } = 0.3;
        public bool UseLLM { get; set; } = true;
        public bool IncludeSources { get; set; } = true;
    }
}
