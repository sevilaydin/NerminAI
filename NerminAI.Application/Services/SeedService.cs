using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using NerminAI.Application.DTOs;
using NerminAI.Application.Interfaces;
using NerminAI.Domain.Entities;
using NerminAI.Domain.Enums;
using NerminAI.Domain.Interfaces;

namespace NerminAI.Application.Services
{
    public class SeedService : ISeedService
    {
        private const int MaxImportedConversationChars = 20000;
        private readonly IDocumentRepository _documentRepository;
        private readonly IChunkRepository _chunkRepository;
        private readonly IVectorRepository _vectorRepository;
        private readonly IEmbeddingService _embeddingService;

        public SeedService(
            IDocumentRepository documentRepository,
            IChunkRepository chunkRepository,
            IVectorRepository vectorRepository,
            IEmbeddingService embeddingService)
        {
            _documentRepository = documentRepository;
            _chunkRepository = chunkRepository;
            _vectorRepository = vectorRepository;
            _embeddingService = embeddingService;
        }

        public async Task SeedDocumentAsync(SeedRequest request)
        {
            var document = new Document
            {
                Title = request.Title,
                Content = request.Content,
                Type = request.Type,
                Metadata = request.Metadata
            };

            document = await _documentRepository.AddAsync(document);

            // Chunk the content
            var chunks = ChunkText(request.Content, maxTokens: 200);

            for (int i = 0; i < chunks.Count; i++)
            {
                var chunk = new Chunk
                {
                    DocumentId = document.Id,
                    Content = chunks[i],
                    ChunkIndex = i,
                    TokenCount = EstimateTokenCount(chunks[i])
                };

                chunk = await _chunkRepository.AddAsync(chunk);

                // Generate and store embedding
                var embedding = await _embeddingService.GenerateEmbeddingAsync(chunks[i]);
                await _vectorRepository.AddEmbeddingAsync(chunk.Id, embedding, "all-MiniLM-L6-v2");
            }
        }

        public async Task SeedDefaultDataAsync()
        {
            // Check if already seeded
            var existingCount = await _documentRepository.GetDocumentCountAsync();
            if (existingCount > 0) return;

            var defaultDocuments = GetDefaultKnowledgeBase();

            foreach (var doc in defaultDocuments)
            {
                await SeedDocumentAsync(doc);
            }
        }

        public async Task<int> SeedChatExportAsync(string? exportDirectory = null, string? keywordRegex = null, bool includeAll = false)
        {
            var baseDirectory = Directory.GetCurrentDirectory();
            var resolvedDirectory = string.IsNullOrWhiteSpace(exportDirectory)
                ? Path.Combine(baseDirectory, "data", "chatgpt-export")
                : exportDirectory!;

            if (!Path.IsPathRooted(resolvedDirectory))
            {
                resolvedDirectory = Path.GetFullPath(Path.Combine(baseDirectory, resolvedDirectory));
            }

            if (!Directory.Exists(resolvedDirectory))
            {
                throw new DirectoryNotFoundException($"Export directory was not found: {resolvedDirectory}");
            }

            var files = Directory.GetFiles(resolvedDirectory, "conversations-*.json")
                .OrderBy(f => f)
                .ToList();

            var htmlFiles = Directory.GetFiles(resolvedDirectory, "chat.html")
                .OrderBy(f => f)
                .ToList();

            if (!files.Any() && !htmlFiles.Any())
            {
                throw new FileNotFoundException($"No conversations-*.json or chat.html files found in: {resolvedDirectory}");
            }

            Regex? filter = null;
            if (!includeAll)
            {
                if (!string.IsNullOrWhiteSpace(keywordRegex))
                {
                    filter = new Regex(keywordRegex, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
                }
                else
                {
                    filter = new Regex("nermin|nero", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
                }
            }

            var importedCount = 0;

            foreach (var file in files)
            {
                var json = await File.ReadAllTextAsync(file);
                var conversations = JsonSerializer.Deserialize<List<ChatExportConversation>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<ChatExportConversation>();

                foreach (var conversation in conversations)
                {
                    var conversationId = conversation.ConversationId ?? conversation.Id;
                    if (string.IsNullOrWhiteSpace(conversationId))
                    {
                        continue;
                    }

                    var title = string.IsNullOrWhiteSpace(conversation.Title)
                        ? $"Conversation {conversationId}"
                        : conversation.Title.Trim();

                    var content = BuildConversationContent(conversation);
                    if (string.IsNullOrWhiteSpace(content))
                    {
                        continue;
                    }

                    if (filter is not null && !filter.IsMatch($"{title}\n{content}"))
                    {
                        continue;
                    }

                    var duplicate = await _documentRepository.FindAsync(d =>
                        d.Metadata != null &&
                        d.Metadata.Contains($"conversationId={conversationId}"));

                    if (duplicate.Any())
                    {
                        continue;
                    }

                    await SeedDocumentAsync(new SeedRequest
                    {
                        Title = $"Chat Export - {title}",
                        Content = content,
                        Type = DocumentType.Experience,
                        Metadata = $"source=chatgpt-export;file={Path.GetFileName(file)};conversationId={conversationId}"
                    });

                    importedCount++;
                }
            }

            foreach (var htmlFile in htmlFiles)
            {
                var html = await File.ReadAllTextAsync(htmlFile);
                var conversations = TryExtractConversationsFromHtml(html);
                if (conversations.Count == 0)
                {
                    continue;
                }

                foreach (var conversation in conversations)
                {
                    var conversationId = conversation.ConversationId ?? conversation.Id;
                    if (string.IsNullOrWhiteSpace(conversationId))
                    {
                        continue;
                    }

                    var title = string.IsNullOrWhiteSpace(conversation.Title)
                        ? $"Conversation {conversationId}"
                        : conversation.Title.Trim();

                    var content = BuildConversationContent(conversation);
                    if (string.IsNullOrWhiteSpace(content))
                    {
                        continue;
                    }

                    if (filter is not null && !filter.IsMatch($"{title}\n{content}"))
                    {
                        continue;
                    }

                    var duplicate = await _documentRepository.FindAsync(d =>
                        d.Metadata != null &&
                        d.Metadata.Contains($"conversationId={conversationId}"));

                    if (duplicate.Any())
                    {
                        continue;
                    }

                    await SeedDocumentAsync(new SeedRequest
                    {
                        Title = $"Chat Export - {title}",
                        Content = content,
                        Type = DocumentType.Experience,
                        Metadata = $"source=chatgpt-export;file={Path.GetFileName(htmlFile)};conversationId={conversationId}"
                    });

                    importedCount++;
                }
            }

            return importedCount;
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

            // If no chunks were created, add the whole text as one chunk
            if (chunks.Count == 0 && !string.IsNullOrWhiteSpace(text))
                chunks.Add(text.Trim());

            return chunks;
        }

        private static int EstimateTokenCount(string text)
            => (int)(text.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length * 1.3);

        private static string BuildConversationContent(ChatExportConversation conversation)
        {
            if (conversation.Mapping is null || conversation.Mapping.Count == 0)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();
            var orderedMessages = conversation.Mapping.Values
                .Where(n => n.Message is not null)
                .OrderBy(n => n.Message!.CreateTime ?? double.MaxValue)
                .ThenBy(n => n.Id)
                .ToList();

            foreach (var node in orderedMessages)
            {
                var role = node.Message!.Author?.Role;
                if (string.IsNullOrWhiteSpace(role))
                {
                    continue;
                }

                if (role != "user" && role != "assistant")
                {
                    continue;
                }

                var text = ExtractMessageText(node.Message.Content?.Parts);
                if (string.IsNullOrWhiteSpace(text))
                {
                    continue;
                }

                sb.AppendLine($"[{role}] {text}");
                sb.AppendLine();

                if (sb.Length >= MaxImportedConversationChars)
                {
                    sb.AppendLine("[truncated]");
                    break;
                }
            }

            return sb.ToString().Trim();
        }

        private static string ExtractMessageText(JsonElement[]? parts)
        {
            if (parts is null || parts.Length == 0)
            {
                return string.Empty;
            }

            var segments = new List<string>();

            foreach (var part in parts)
            {
                if (part.ValueKind == JsonValueKind.String)
                {
                    var value = part.GetString();
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        segments.Add(value.Trim());
                    }

                    continue;
                }

                if (part.ValueKind == JsonValueKind.Object)
                {
                    if (part.TryGetProperty("text", out var textProp) && textProp.ValueKind == JsonValueKind.String)
                    {
                        var value = textProp.GetString();
                        if (!string.IsNullOrWhiteSpace(value))
                        {
                            segments.Add(value.Trim());
                        }
                    }
                    else if (part.TryGetProperty("prompt", out var promptProp) && promptProp.ValueKind == JsonValueKind.String)
                    {
                        var value = promptProp.GetString();
                        if (!string.IsNullOrWhiteSpace(value))
                        {
                            segments.Add(value.Trim());
                        }
                    }
                }
            }

            return string.Join("\n", segments).Trim();
        }

        private static List<ChatExportConversation> TryExtractConversationsFromHtml(string html)
        {
            if (string.IsNullOrWhiteSpace(html))
            {
                return new List<ChatExportConversation>();
            }

            const string marker = "var jsonData =";
            var markerIndex = html.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
            if (markerIndex < 0)
            {
                return new List<ChatExportConversation>();
            }

            var startIndex = html.IndexOf('[', markerIndex);
            if (startIndex < 0)
            {
                return new List<ChatExportConversation>();
            }

            var endIndex = html.IndexOf("];", startIndex, StringComparison.Ordinal);
            if (endIndex < 0)
            {
                return new List<ChatExportConversation>();
            }

            var json = html.Substring(startIndex, endIndex - startIndex + 1);
            try
            {
                return JsonSerializer.Deserialize<List<ChatExportConversation>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<ChatExportConversation>();
            }
            catch
            {
                return new List<ChatExportConversation>();
            }
        }

        private static List<SeedRequest> GetDefaultKnowledgeBase()
        {
            return new List<SeedRequest>
            {
                new()
                {
                    Title = "Nermin - Professional Summary",
                    Content = @"Nermin is a software engineer with a strong focus on .NET technologies and backend development.
She is passionate about building scalable, maintainable software systems using Clean Architecture principles.
Her expertise spans C#, .NET 8, ASP.NET Core Web API, Entity Framework Core, and PostgreSQL.
She has experience with microservices architecture, Docker containerization, and message queue systems like RabbitMQ.
Nermin is also exploring AI integration into classical backend systems, including RAG (Retrieval-Augmented Generation) and vector search.",
                    Type = DocumentType.CV
                },
                new()
                {
                    Title = "NerminAI Project",
                    Content = @"NerminAI is an AI-powered knowledge engine built with .NET 8 and Clean Architecture.
It uses RAG (Retrieval-Augmented Generation) to answer questions about Nermin's professional profile.
The tech stack includes: ASP.NET Core Web API, PostgreSQL with pgvector extension for vector similarity search,
Groq API with llama-3.3-70b-versatile model for natural language generation,
all-MiniLM-L6-v2 model for 384-dimensional text embeddings,
Streamlit for the chat UI frontend.
Features include: question answering with source citations, project effort estimation, seed data management, and health monitoring.
The project demonstrates Clean Architecture with Domain, Application, Infrastructure, and API layers.",
                    Type = DocumentType.Project
                },
                new()
                {
                    Title = "Technical Skills",
                    Content = @"Programming Languages: C#, Python, SQL
Frameworks: .NET 8, ASP.NET Core Web API, Entity Framework Core 9
Databases: PostgreSQL 16, SQL Server, pgvector extension
Architecture: Clean Architecture, Microservices, CQRS, Event-Driven Architecture
DevOps: Docker, Docker Compose, Git, GitHub Actions
AI/ML: RAG (Retrieval-Augmented Generation), Vector Search, Embeddings, Groq API, LLM Integration
Message Queues: RabbitMQ
Testing: xUnit, FluentAssertions, Moq
Tools: Visual Studio, VS Code, Swagger/OpenAPI, Postman
Design Patterns: Repository Pattern, Unit of Work, Factory, Strategy, Mediator",
                    Type = DocumentType.Skill
                },
                new()
                {
                    Title = "Education",
                    Content = @"Nermin has a strong educational background in engineering and computer science.
She continuously improves her skills through online courses, technical documentation, and hands-on projects.
She follows best practices from industry leaders and keeps up with the latest .NET and backend development trends.",
                    Type = DocumentType.Education
                },
                new()
                {
                    Title = "Career Goals",
                    Content = @"Nermin's career goals include:
- Becoming a Solution/Backend Architect for mission-critical systems
- Designing event-driven and data-intensive distributed platforms
- Building hybrid systems combining AI with classical backend architectures
- Contributing to open-source .NET projects
- Mastering cloud-native development with Kubernetes and service mesh technologies
- Leading technical teams and mentoring junior developers",
                    Type = DocumentType.CV
                }
            };
        }

        private sealed class ChatExportConversation
        {
            public string? Id { get; set; }
            [JsonPropertyName("conversation_id")]
            public string? ConversationId { get; set; }
            public string? Title { get; set; }
            public Dictionary<string, ChatExportNode>? Mapping { get; set; }
        }

        private sealed class ChatExportNode
        {
            public string? Id { get; set; }
            public ChatExportMessage? Message { get; set; }
        }

        private sealed class ChatExportMessage
        {
            public ChatExportAuthor? Author { get; set; }
            public ChatExportContent? Content { get; set; }
            [JsonPropertyName("create_time")]
            public double? CreateTime { get; set; }
        }

        private sealed class ChatExportAuthor
        {
            public string? Role { get; set; }
        }

        private sealed class ChatExportContent
        {
            public JsonElement[]? Parts { get; set; }
        }
    }
}
