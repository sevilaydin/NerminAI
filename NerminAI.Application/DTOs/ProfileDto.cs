namespace NerminAI.Application.DTOs
{
    public class ProfileDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? BirthDate { get; set; }
        public string Bio { get; set; } = string.Empty;
        public string? Location { get; set; }
        public string ToneStyle { get; set; } = string.Empty;
        public string SpeakingStyle { get; set; } = string.Empty;
        public string[] PersonalityTraits { get; set; } = Array.Empty<string>();
    }
}
