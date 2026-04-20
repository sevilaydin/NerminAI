using NerminAI.Domain.Enums;

namespace NerminAI.Domain.Entities
{
    public class Memory : BaseEntity
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime? MemoryDate { get; set; }
        public EmotionalTone EmotionalTone { get; set; } = EmotionalTone.Neutral;
        public MemoryCategory Category { get; set; } = MemoryCategory.Daily;

        /// <summary>
        /// FK to the shadow Document created for RAG embedding purposes.
        /// </summary>
        public Guid? ShadowDocumentId { get; set; }
    }
}
