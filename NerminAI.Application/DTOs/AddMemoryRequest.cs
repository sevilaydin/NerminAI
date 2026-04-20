using NerminAI.Domain.Enums;

namespace NerminAI.Application.DTOs
{
    public class AddMemoryRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime? MemoryDate { get; set; }
        public EmotionalTone EmotionalTone { get; set; } = EmotionalTone.Neutral;
        public MemoryCategory Category { get; set; } = MemoryCategory.Daily;
    }
}
