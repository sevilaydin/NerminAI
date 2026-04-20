namespace NerminAI.Application.DTOs
{
    public class SeedChatExportRequest
    {
        public string? ExportDirectory { get; set; }
        public string? KeywordRegex { get; set; }
        public bool IncludeAll { get; set; }
    }
}
