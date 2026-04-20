namespace NerminAI.Domain.Entities
{
    public class DailyRoutine : BaseEntity
    {
        public string TimeOfDay { get; set; } = string.Empty; // Morning / Afternoon / Evening / Night
        public string Activity { get; set; } = string.Empty;
        public string Frequency { get; set; } = string.Empty; // Daily / Weekdays / Weekends
        public string? Notes { get; set; }
        public Guid? ShadowDocumentId { get; set; }
    }
}
