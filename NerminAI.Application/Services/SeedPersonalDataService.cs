using NerminAI.Application.DTOs;
using NerminAI.Application.Interfaces;
using NerminAI.Domain.Entities;
using NerminAI.Domain.Enums;
using NerminAI.Domain.Interfaces;

namespace NerminAI.Application.Services
{
    /// <summary>
    /// Dual-write engine: stores personal entities in their own tables AND
    /// creates shadow Documents+Chunks+Embeddings for unified RAG retrieval.
    /// </summary>
    public class SeedPersonalDataService : ISeedPersonalDataService
    {
        private readonly IPersonProfileRepository _profileRepo;
        private readonly IMemoryRepository _memoryRepo;
        private readonly IPreferenceRepository _prefRepo;
        private readonly IRelationshipRepository _relRepo;
        private readonly ILifeEventRepository _eventRepo;
        private readonly IInterestRepository _interestRepo;
        private readonly IDocumentRepository _documentRepo;
        private readonly IChunkRepository _chunkRepo;
        private readonly IVectorRepository _vectorRepo;
        private readonly IEmbeddingService _embeddingService;

        public SeedPersonalDataService(
            IPersonProfileRepository profileRepo,
            IMemoryRepository memoryRepo,
            IPreferenceRepository prefRepo,
            IRelationshipRepository relRepo,
            ILifeEventRepository eventRepo,
            IInterestRepository interestRepo,
            IDocumentRepository documentRepo,
            IChunkRepository chunkRepo,
            IVectorRepository vectorRepo,
            IEmbeddingService embeddingService)
        {
            _profileRepo = profileRepo;
            _memoryRepo = memoryRepo;
            _prefRepo = prefRepo;
            _relRepo = relRepo;
            _eventRepo = eventRepo;
            _interestRepo = interestRepo;
            _documentRepo = documentRepo;
            _chunkRepo = chunkRepo;
            _vectorRepo = vectorRepo;
            _embeddingService = embeddingService;
        }

        public async Task SeedAsync(SeedPersonalDataRequest request)
        {
            if (request.Profile is not null)
                await SeedProfileAsync(request.Profile);

            foreach (var m in request.Memories)
                await SeedMemoryAsync(m);

            foreach (var p in request.Preferences)
                await SeedPreferenceAsync(p);

            foreach (var r in request.Relationships)
                await SeedRelationshipAsync(r);

            foreach (var e in request.Events)
                await SeedEventAsync(e);

            foreach (var i in request.Interests)
                await SeedInterestAsync(i);

            foreach (var d in request.DailyRoutines)
                await SeedDailyRoutineAsync(d);
        }

        private async Task SeedProfileAsync(PersonProfileSeedDto dto)
        {
            // Deactivate existing active profile
            var existing = await _profileRepo.GetActiveProfileAsync();
            if (existing is not null)
            {
                existing.IsActive = false;
                await _profileRepo.UpdateAsync(existing);
            }

            DateOnly? birthDate = null;
            if (!string.IsNullOrEmpty(dto.BirthDate) && DateOnly.TryParse(dto.BirthDate, out var parsed))
                birthDate = parsed;

            var profile = new PersonProfile
            {
                FullName = dto.FullName,
                BirthDate = birthDate,
                Bio = dto.Bio,
                Location = dto.Location,
                ToneStyle = dto.ToneStyle,
                SpeakingStyle = dto.SpeakingStyle,
                PersonalityTraits = dto.PersonalityTraits,
                IsActive = true
            };

            profile = await _profileRepo.AddAsync(profile);

            var text = $"[Kişisel Profil] {profile.FullName}\n" +
                       $"Biyografi: {profile.Bio}\n" +
                       (profile.Location is not null ? $"Konum: {profile.Location}\n" : "") +
                       $"Ton stili: {profile.ToneStyle}\n" +
                       $"Kişilik özellikleri: {string.Join(", ", profile.PersonalityTraits)}";

            var shadowDoc = await SeedShadowDocumentAsync(
                $"Profil - {profile.FullName}",
                text,
                DocumentType.PersonalProfile,
                $"source=personal;entityType=PersonProfile;entityId={profile.Id}");

            profile.ShadowDocumentId = shadowDoc.Id;
            await _profileRepo.UpdateAsync(profile);
        }

        private async Task SeedMemoryAsync(MemorySeedDto dto)
        {
            DateTime? memoryDate = null;
            if (!string.IsNullOrEmpty(dto.Date) && DateTime.TryParse(dto.Date, out var parsed))
                memoryDate = parsed;

            var memory = new Memory
            {
                Title = dto.Title,
                Content = dto.Content,
                MemoryDate = memoryDate,
                EmotionalTone = dto.EmotionalTone,
                Category = dto.Category
            };

            memory = await _memoryRepo.AddAsync(memory);

            var text = $"[Anı - {memory.Category}] {memory.Title}\n" +
                       $"Tarih: {memory.MemoryDate?.ToString("yyyy-MM-dd") ?? "bilinmiyor"}\n" +
                       $"Duygusal ton: {memory.EmotionalTone}\n\n" +
                       memory.Content;

            var shadowDoc = await SeedShadowDocumentAsync(
                $"Anı - {memory.Title}",
                text,
                DocumentType.PersonalMemory,
                $"source=personal;entityType=Memory;entityId={memory.Id}");

            memory.ShadowDocumentId = shadowDoc.Id;
            await _memoryRepo.UpdateAsync(memory);
        }

        private async Task SeedPreferenceAsync(PreferenceSeedDto dto)
        {
            var preference = new Preference
            {
                Category = dto.Category,
                Item = dto.Item,
                Strength = dto.Strength,
                Context = dto.Context
            };

            preference = await _prefRepo.AddAsync(preference);

            var text = $"[Tercih - {preference.Category}] {preference.Item}\n" +
                       $"Güç: {preference.Strength}\n" +
                       (preference.Context is not null ? $"Bağlam: {preference.Context}" : "");

            var shadowDoc = await SeedShadowDocumentAsync(
                $"Tercih - {preference.Category}: {preference.Item}",
                text,
                DocumentType.PersonalPreference,
                $"source=personal;entityType=Preference;entityId={preference.Id}");

            preference.ShadowDocumentId = shadowDoc.Id;
            await _prefRepo.UpdateAsync(preference);
        }

        private async Task SeedRelationshipAsync(RelationshipSeedDto dto)
        {
            var relationship = new Relationship
            {
                Name = dto.Name,
                Type = dto.Type,
                Description = dto.Description,
                ClosenessLevel = dto.ClosenessLevel
            };

            relationship = await _relRepo.AddAsync(relationship);

            var text = $"[İlişki - {relationship.Type}] {relationship.Name}\n" +
                       $"Yakınlık seviyesi: {relationship.ClosenessLevel}/10\n" +
                       (relationship.Description is not null ? relationship.Description : "");

            var shadowDoc = await SeedShadowDocumentAsync(
                $"İlişki - {relationship.Name}",
                text,
                DocumentType.PersonalRelationship,
                $"source=personal;entityType=Relationship;entityId={relationship.Id}");

            relationship.ShadowDocumentId = shadowDoc.Id;
            await _relRepo.UpdateAsync(relationship);
        }

        private async Task SeedEventAsync(LifeEventSeedDto dto)
        {
            DateTime eventDate = DateTime.UtcNow;
            if (!string.IsNullOrEmpty(dto.Date) && DateTime.TryParse(dto.Date, out var parsed))
                eventDate = parsed;

            var lifeEvent = new LifeEvent
            {
                Title = dto.Title,
                Date = eventDate,
                Location = dto.Location,
                Description = dto.Description,
                EmotionalSignificance = dto.EmotionalSignificance,
                Tags = dto.Tags
            };

            lifeEvent = await _eventRepo.AddAsync(lifeEvent);

            var text = $"[Yaşam Olayı] {lifeEvent.Title}\n" +
                       $"Tarih: {lifeEvent.Date:yyyy-MM-dd}\n" +
                       $"Yer: {lifeEvent.Location ?? "belirtilmemiş"}\n" +
                       $"Duygusal önem: {lifeEvent.EmotionalSignificance}/10\n" +
                       $"Etiketler: {string.Join(", ", lifeEvent.Tags)}\n\n" +
                       lifeEvent.Description;

            var shadowDoc = await SeedShadowDocumentAsync(
                $"Olay - {lifeEvent.Title}",
                text,
                DocumentType.PersonalEvent,
                $"source=personal;entityType=LifeEvent;entityId={lifeEvent.Id}");

            lifeEvent.ShadowDocumentId = shadowDoc.Id;
            await _eventRepo.UpdateAsync(lifeEvent);
        }

        private async Task SeedInterestAsync(InterestSeedDto dto)
        {
            var interest = new Interest
            {
                Name = dto.Name,
                Level = dto.Level,
                Description = dto.Description,
                RelatedMemoryTitles = dto.RelatedMemoryTitles
            };

            interest = await _interestRepo.AddAsync(interest);

            var text = $"[İlgi Alanı] {interest.Name}\n" +
                       $"Seviye: {interest.Level}\n" +
                       (interest.Description is not null ? interest.Description : "") +
                       (interest.RelatedMemoryTitles.Any()
                           ? $"\nİlgili anılar: {string.Join(", ", interest.RelatedMemoryTitles)}"
                           : "");

            var shadowDoc = await SeedShadowDocumentAsync(
                $"İlgi - {interest.Name}",
                text,
                DocumentType.PersonalInterest,
                $"source=personal;entityType=Interest;entityId={interest.Id}");

            interest.ShadowDocumentId = shadowDoc.Id;
            await _interestRepo.UpdateAsync(interest);
        }

        private async Task SeedDailyRoutineAsync(DailyRoutineSeedDto dto)
        {
            var routine = new DailyRoutine
            {
                TimeOfDay = dto.TimeOfDay,
                Activity = dto.Activity,
                Frequency = dto.Frequency,
                Notes = dto.Notes
            };

            routine = await new GenericRoutineAdder(_documentRepo, _chunkRepo, _vectorRepo, _embeddingService)
                .AddAndReturnAsync(routine, dto);

            // For DailyRoutine we don't store in a dedicated repo via interface (no IDailyRoutineRepository)
            // The shadow doc is enough for RAG retrieval
        }

        private async Task<Document> SeedShadowDocumentAsync(
            string title, string content, DocumentType type, string metadata)
        {
            var document = new Document
            {
                Title = title,
                Content = content,
                Type = type,
                Metadata = metadata
            };

            document = await _documentRepo.AddAsync(document);

            var chunks = ChunkText(content, maxTokens: 200);
            for (int i = 0; i < chunks.Count; i++)
            {
                var chunk = new Chunk
                {
                    DocumentId = document.Id,
                    Content = chunks[i],
                    ChunkIndex = i,
                    TokenCount = EstimateTokenCount(chunks[i])
                };

                chunk = await _chunkRepo.AddAsync(chunk);

                var embedding = await _embeddingService.GenerateEmbeddingAsync(chunks[i]);
                await _vectorRepo.AddEmbeddingAsync(chunk.Id, embedding, "all-MiniLM-L6-v2");
            }

            return document;
        }

        private static List<string> ChunkText(string text, int maxTokens = 200)
        {
            var chunks = new List<string>();
            var paragraphs = text.Split(new[] { "\n\n", "\r\n\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            var currentChunk = "";
            foreach (var para in paragraphs)
            {
                if (EstimateTokenCount(currentChunk + "\n\n" + para) > maxTokens && !string.IsNullOrWhiteSpace(currentChunk))
                {
                    chunks.Add(currentChunk.Trim());
                    currentChunk = para;
                }
                else
                {
                    currentChunk = string.IsNullOrWhiteSpace(currentChunk) ? para : currentChunk + "\n\n" + para;
                }
            }

            if (!string.IsNullOrWhiteSpace(currentChunk))
                chunks.Add(currentChunk.Trim());

            if (chunks.Count == 0 && !string.IsNullOrWhiteSpace(text))
                chunks.Add(text.Trim());

            return chunks;
        }

        private static int EstimateTokenCount(string text)
            => (int)(text.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length * 1.3);

        // Simple inline helper to handle DailyRoutine without a dedicated repository interface
        private sealed class GenericRoutineAdder
        {
            private readonly IDocumentRepository _documentRepo;
            private readonly IChunkRepository _chunkRepo;
            private readonly IVectorRepository _vectorRepo;
            private readonly IEmbeddingService _embeddingService;

            public GenericRoutineAdder(
                IDocumentRepository documentRepo,
                IChunkRepository chunkRepo,
                IVectorRepository vectorRepo,
                IEmbeddingService embeddingService)
            {
                _documentRepo = documentRepo;
                _chunkRepo = chunkRepo;
                _vectorRepo = vectorRepo;
                _embeddingService = embeddingService;
            }

            public async Task<DailyRoutine> AddAndReturnAsync(DailyRoutine routine, DailyRoutineSeedDto dto)
            {
                var text = $"[Günlük Rutin - {dto.TimeOfDay}] {dto.Activity}\n" +
                           $"Sıklık: {dto.Frequency}\n" +
                           (dto.Notes is not null ? $"Notlar: {dto.Notes}" : "");

                var document = new Document
                {
                    Title = $"Rutin - {dto.TimeOfDay}: {dto.Activity}",
                    Content = text,
                    Type = DocumentType.PersonalRoutine,
                    Metadata = $"source=personal;entityType=DailyRoutine"
                };

                document = await _documentRepo.AddAsync(document);

                var chunks = ChunkText(text);
                for (int i = 0; i < chunks.Count; i++)
                {
                    var chunk = new Chunk
                    {
                        DocumentId = document.Id,
                        Content = chunks[i],
                        ChunkIndex = i,
                        TokenCount = EstimateTokenCount(chunks[i])
                    };
                    chunk = await _chunkRepo.AddAsync(chunk);
                    var embedding = await _embeddingService.GenerateEmbeddingAsync(chunks[i]);
                    await _vectorRepo.AddEmbeddingAsync(chunk.Id, embedding, "all-MiniLM-L6-v2");
                }

                routine.ShadowDocumentId = document.Id;
                return routine;
            }

            private static List<string> ChunkText(string text, int maxTokens = 200)
            {
                var chunks = new List<string>();
                var paragraphs = text.Split(new[] { "\n\n", "\r\n\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                var currentChunk = "";
                foreach (var para in paragraphs)
                {
                    if (EstimateTokenCount(currentChunk + "\n\n" + para) > maxTokens && !string.IsNullOrWhiteSpace(currentChunk))
                    {
                        chunks.Add(currentChunk.Trim());
                        currentChunk = para;
                    }
                    else
                    {
                        currentChunk = string.IsNullOrWhiteSpace(currentChunk) ? para : currentChunk + "\n\n" + para;
                    }
                }
                if (!string.IsNullOrWhiteSpace(currentChunk)) chunks.Add(currentChunk.Trim());
                if (chunks.Count == 0 && !string.IsNullOrWhiteSpace(text)) chunks.Add(text.Trim());
                return chunks;
            }

            private static int EstimateTokenCount(string text)
                => (int)(text.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length * 1.3);
        }
    }
}
