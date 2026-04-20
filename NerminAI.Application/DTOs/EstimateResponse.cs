namespace NerminAI.Application.DTOs
{
    public class EstimateResponse
    {
        public string Estimate { get; set; } = string.Empty;
        public long LatencyMs { get; set; }
    }
}
