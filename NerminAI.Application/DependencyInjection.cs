using Microsoft.Extensions.DependencyInjection;
using NerminAI.Application.Interfaces;
using NerminAI.Application.Services;

namespace NerminAI.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // Existing services
            services.AddScoped<IAskService, AskService>();
            services.AddScoped<ISeedService, SeedService>();
            services.AddScoped<IEstimateService, EstimateService>();
            services.AddScoped<IHealthService, HealthService>();

            // Personal AI services
            services.AddScoped<ISeedPersonalDataService, SeedPersonalDataService>();
            services.AddScoped<IPersonalQuestionService, PersonalQuestionService>();
            services.AddScoped<IMemoryService, MemoryService>();
            services.AddScoped<IProfileService, ProfileService>();
            services.AddScoped<IRecommendService, RecommendService>();

            return services;
        }
    }
}
