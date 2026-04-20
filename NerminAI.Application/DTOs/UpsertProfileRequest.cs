namespace NerminAI.Application.DTOs
{
    public class UpsertProfileRequest
    {
        public string FullName { get; set; } = string.Empty;
        public string? BirthDate { get; set; } // ISO date string e.g. "1998-06-15"
        public string Bio { get; set; } = string.Empty;
        public string? Location { get; set; }
        public string ToneStyle { get; set; } = "warm and conversational";
        public string SpeakingStyle { get; set; } = "first-person";
        public string[] PersonalityTraits { get; set; } = Array.Empty<string>();
    }
}
