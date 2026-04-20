namespace NerminAI.Domain.Entities
{
    // Named LifeEvent to avoid collision with System.EventHandler delegates
    public class LifeEvent : BaseEntity
    {
        public string Title { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string? Location { get; set; }
        public string Description { get; set; } = string.Empty;
        public int EmotionalSignificance { get; set; } = 5; // 1-10
        public string[] Tags { get; set; } = Array.Empty<string>();
        public Guid? ShadowDocumentId { get; set; }
    }
}
