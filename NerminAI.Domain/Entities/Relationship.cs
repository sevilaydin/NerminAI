using NerminAI.Domain.Enums;

namespace NerminAI.Domain.Entities
{
    public class Relationship : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public RelationshipType Type { get; set; }
        public string? Description { get; set; }
        public int ClosenessLevel { get; set; } = 5; // 1-10
        public Guid? ShadowDocumentId { get; set; }
    }
}
