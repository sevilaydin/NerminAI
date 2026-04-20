using NerminAI.Domain.Enums;

namespace NerminAI.Application.DTOs
{
    public class SeedRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DocumentType Type { get; set; }
        public string? Metadata { get; set; }
    }
}
