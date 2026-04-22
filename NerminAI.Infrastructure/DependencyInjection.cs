using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NerminAI.Domain.Interfaces;
using NerminAI.Infrastructure.Data;
using NerminAI.Infrastructure.Repositories;
using NerminAI.Infrastructure.Services;

namespace NerminAI.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Database
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(
                    configuration.GetConnectionString("DefaultConnection"),
                    npgsqlOptions => { }));

            // Repositories
            services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IDocumentRepository, DocumentRepository>();
            services.AddScoped<IChunkRepository, ChunkRepository>();
            services.AddScoped<IVectorRepository, PersonalMemoryVectorRepository>();

            // Personal AI repositories
            services.AddScoped<IPersonProfileRepository, PersonProfileRepository>();
            services.AddScoped<IMemoryRepository, MemoryRepository>();
            services.AddScoped<IPreferenceRepository, PreferenceRepository>();
            services.AddScoped<IRelationshipRepository, RelationshipRepository>();
            services.AddScoped<ILifeEventRepository, LifeEventRepository>();
            services.AddScoped<IInterestRepository, InterestRepository>();

            // Embedding Service
            var embeddingUrl = configuration["EmbeddingSettings:BaseUrl"] ?? "http://localhost:8000";
            var embeddingModel = configuration["EmbeddingSettings:Model"] ?? "all-MiniLM-L6-v2";
            services.AddHttpClient<IEmbeddingService, LocalEmbeddingService>(client =>
            {
                client.BaseAddress = new Uri(embeddingUrl);
                client.Timeout = TimeSpan.FromSeconds(30);
            })
            .ConfigureHttpClient((sp, client) => { })
            .AddTypedClient<IEmbeddingService>((client, sp) =>
                new LocalEmbeddingService(client, embeddingModel));

            // LLM Service
            var groqApiKey = configuration["LLMSettings:Groq:ApiKey"] ?? "";
            var groqModel = configuration["LLMSettings:Groq:Model"] ?? "llama-3.3-70b-versatile";
            services.AddHttpClient<ILLMService, GroqLLMService>(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(60);
            })
            .AddTypedClient<ILLMService>((client, sp) =>
                new GroqLLMService(client, groqApiKey, groqModel));

            return services;
        }
    }
}
