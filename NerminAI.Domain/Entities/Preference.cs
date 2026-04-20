using NerminAI.Domain.Enums;

namespace NerminAI.Domain.Entities
{
    public class Preference : BaseEntity
    {
        public string Category { get; set; } = string.Empty;
        public string Item { get; set; } = string.Empty;
        public PreferenceStrength Strength { get; set; } = PreferenceStrength.Moderate;
        public string? Context { get; set; }
        public Guid? ShadowDocumentId { get; set; }
    }
}
