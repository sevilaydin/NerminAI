using NerminAI.Domain.Enums;

namespace NerminAI.Application.DTOs
{
    public class MemoryReflectionRequest
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public MemoryCategory? Category { get; set; }
        public EmotionalTone? EmotionalTone { get; set; }

        /// <summary>
        /// Optional free-form prompt for semantic search.
        /// Example: "reflect on my happiest childhood memories"
        /// </summary>
        public string? FreeFormPrompt { get; set; }
    }
}
