using NerminAI.Domain.Enums;

namespace NerminAI.Application.DTOs
{
    public class SeedPersonalDataRequest
    {
        public PersonProfileSeedDto? Profile { get; set; }
        public List<MemorySeedDto> Memories { get; set; } = new();
        public List<PreferenceSeedDto> Preferences { get; set; } = new();
        public List<RelationshipSeedDto> Relationships { get; set; } = new();
        public List<LifeEventSeedDto> Events { get; set; } = new();
        public List<InterestSeedDto> Interests { get; set; } = new();
        public List<DailyRoutineSeedDto> DailyRoutines { get; set; } = new();
    }

    public class PersonProfileSeedDto
    {
        public string FullName { get; set; } = string.Empty;
        public string? BirthDate { get; set; }
        public string Bio { get; set; } = string.Empty;
        public string? Location { get; set; }
        public string ToneStyle { get; set; } = "warm and conversational";
        public string SpeakingStyle { get; set; } = "first-person";
        public string[] PersonalityTraits { get; set; } = Array.Empty<string>();
    }

    public class MemorySeedDto
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? Date { get; set; }
        public EmotionalTone EmotionalTone { get; set; } = EmotionalTone.Neutral;
        public MemoryCategory Category { get; set; } = MemoryCategory.Daily;
    }

    public class PreferenceSeedDto
    {
        public string Category { get; set; } = string.Empty;
        public string Item { get; set; } = string.Empty;
        public PreferenceStrength Strength { get; set; } = PreferenceStrength.Moderate;
        public string? Context { get; set; }
    }

    public class RelationshipSeedDto
    {
        public string Name { get; set; } = string.Empty;
        public RelationshipType Type { get; set; }
        public string? Description { get; set; }
        public int ClosenessLevel { get; set; } = 5;
    }

    public class LifeEventSeedDto
    {
        public string Title { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;
        public string? Location { get; set; }
        public string Description { get; set; } = string.Empty;
        public int EmotionalSignificance { get; set; } = 5;
        public string[] Tags { get; set; } = Array.Empty<string>();
    }

    public class InterestSeedDto
    {
        public string Name { get; set; } = string.Empty;
        public InterestLevel Level { get; set; } = InterestLevel.Interested;
        public string? Description { get; set; }
        public string[] RelatedMemoryTitles { get; set; } = Array.Empty<string>();
    }

    public class DailyRoutineSeedDto
    {
        public string TimeOfDay { get; set; } = string.Empty;
        public string Activity { get; set; } = string.Empty;
        public string Frequency { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }
}
