namespace NerminAI.Domain.Entities
{
    public class PersonProfile : BaseEntity
    {
        public string FullName { get; set; } = string.Empty;
        public DateOnly? BirthDate { get; set; }
        public string Bio { get; set; } = string.Empty;
        public string? Location { get; set; }

        /// <summary>
        /// Describes the tone in which the AI should respond.
        /// Example: "warm, direct, slightly humorous"
        /// </summary>
        public string ToneStyle { get; set; } = "warm and conversational";

        /// <summary>
        /// Perspective used when the AI speaks.
        /// Example: "first-person"
        /// </summary>
        public string SpeakingStyle { get; set; } = "first-person";

        /// <summary>
        /// Key personality traits stored as a PostgreSQL text[] array.
        /// Example: ["curious", "determined", "empathetic"]
        /// </summary>
        public string[] PersonalityTraits { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Only one profile may be active at a time.
        /// Enforced by a partial unique index in EF configuration.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// FK to the shadow Document created for RAG embedding purposes.
        /// </summary>
        public Guid? ShadowDocumentId { get; set; }
    }
}
