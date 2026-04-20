namespace NerminAI.Domain.Enums
{
    public enum DocumentType
    {
        CV = 1,
        Project = 2,
        Experience = 3,
        Education = 4,
        Skill = 5,

        // Personal entity shadow documents (used for unified RAG pipeline)
        PersonalProfile = 10,
        PersonalMemory = 11,
        PersonalPreference = 12,
        PersonalRelationship = 13,
        PersonalEvent = 14,
        PersonalInterest = 15,
        PersonalRoutine = 16
    }
}
