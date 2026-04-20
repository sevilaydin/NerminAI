using System.Diagnostics;
using System.Text;
using NerminAI.Application.DTOs;
using NerminAI.Application.Interfaces;
using NerminAI.Domain.Interfaces;

namespace NerminAI.Application.Services
{
    public class ProfileService : IProfileService
    {
        private readonly ISeedPersonalDataService _seedService;
        private readonly IPersonProfileRepository _profileRepository;
        private readonly IInterestRepository _interestRepository;
        private readonly IRelationshipRepository _relationshipRepository;
        private readonly ILLMService _llmService;

        public ProfileService(
            ISeedPersonalDataService seedService,
            IPersonProfileRepository profileRepository,
            IInterestRepository interestRepository,
            IRelationshipRepository relationshipRepository,
            ILLMService llmService)
        {
            _seedService = seedService;
            _profileRepository = profileRepository;
            _interestRepository = interestRepository;
            _relationshipRepository = relationshipRepository;
            _llmService = llmService;
        }

        public async Task<ProfileDto?> GetActiveProfileAsync()
        {
            var profile = await _profileRepository.GetActiveProfileAsync();
            if (profile is null) return null;

            return new ProfileDto
            {
                Id = profile.Id,
                FullName = profile.FullName,
                BirthDate = profile.BirthDate?.ToString("yyyy-MM-dd"),
                Bio = profile.Bio,
                Location = profile.Location,
                ToneStyle = profile.ToneStyle,
                SpeakingStyle = profile.SpeakingStyle,
                PersonalityTraits = profile.PersonalityTraits
            };
        }

        public async Task UpsertProfileAsync(UpsertProfileRequest request)
        {
            var seedRequest = new SeedPersonalDataRequest
            {
                Profile = new PersonProfileSeedDto
                {
                    FullName = request.FullName,
                    BirthDate = request.BirthDate,
                    Bio = request.Bio,
                    Location = request.Location,
                    ToneStyle = request.ToneStyle,
                    SpeakingStyle = request.SpeakingStyle,
                    PersonalityTraits = request.PersonalityTraits
                }
            };

            await _seedService.SeedAsync(seedRequest);
        }

        public async Task<PersonalitySummaryResponse> GetPersonalitySummaryAsync()
        {
            var sw = Stopwatch.StartNew();

            var profile = await _profileRepository.GetActiveProfileAsync();
            var allInterests = (await _interestRepository.GetAllAsync()).ToList();
            var allRelationships = (await _relationshipRepository.GetAllAsync()).ToList();

            var interestBreakdown = allInterests
                .GroupBy(i => i.Level.ToString())
                .ToDictionary(g => g.Key, g => g.Count());

            var relationshipBreakdown = allRelationships
                .GroupBy(r => r.Type.ToString())
                .ToDictionary(g => g.Key, g => g.Count());

            string narrative;
            if (_llmService.IsAvailable && profile is not null)
            {
                var systemPrompt = "Sen kişilik analizi yapan bir AI asistanısın. Verilen bilgilere dayanarak samimi ve içgörülü bir kişilik özeti oluştur.";

                var sb = new StringBuilder();
                sb.AppendLine($"İsim: {profile.FullName}");
                sb.AppendLine($"Biyografi: {profile.Bio}");
                sb.AppendLine($"Kişilik özellikleri: {string.Join(", ", profile.PersonalityTraits)}");
                sb.AppendLine($"Ton stili: {profile.ToneStyle}");
                sb.AppendLine($"İlgi alanları: {string.Join(", ", allInterests.Select(i => $"{i.Name} ({i.Level})"))}");
                sb.AppendLine($"İlişki türleri: {string.Join(", ", allRelationships.Select(r => $"{r.Name} ({r.Type})"))}");

                narrative = await _llmService.GeneratePersonalAnswerAsync(
                    "Bu kişi için kısa ve samimi bir kişilik özeti yaz.",
                    systemPrompt,
                    new[] { sb.ToString() });
            }
            else
            {
                narrative = profile is not null
                    ? $"{profile.FullName} — {profile.Bio}"
                    : "Profil bulunamadı.";
            }

            sw.Stop();

            return new PersonalitySummaryResponse
            {
                FullName = profile?.FullName ?? "Bilinmiyor",
                Traits = profile?.PersonalityTraits ?? Array.Empty<string>(),
                ToneStyle = profile?.ToneStyle ?? string.Empty,
                InterestBreakdown = interestBreakdown,
                RelationshipBreakdown = relationshipBreakdown,
                LLMNarrative = narrative,
                LatencyMs = sw.ElapsedMilliseconds
            };
        }
    }
}
