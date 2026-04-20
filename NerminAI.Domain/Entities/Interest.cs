using NerminAI.Domain.Enums;

namespace NerminAI.Domain.Entities
{
    public class Interest : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public InterestLevel Level { get; set; } = InterestLevel.Interested;
        public string? Description { get; set; }
        public string[] RelatedMemoryTitles { get; set; } = Array.Empty<string>();
        public Guid? ShadowDocumentId { get; set; }
    }
}
