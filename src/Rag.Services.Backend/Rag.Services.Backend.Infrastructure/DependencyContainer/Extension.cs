using Microsoft.Extensions.DependencyInjection;
using Rag.Services.Backend.Application.Services;
using Rag.Services.Backend.Application.Interfaces.Services;
using Rag.Services.Backend.Infrastructure.Services;

namespace Rag.Services.Backend.Infrastructure.DependencyContainer
{
    public static class Extension
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // Repositories

            // Services
            services.AddMemoryCache();
            services.AddHttpClient();
            services.AddScoped<IPdfLoaderService, PdfLoaderService>();
            services.AddScoped<IWordLoaderService, WordLoaderService>();
            services.AddScoped<IQdrantService, QdrantService>();
            services.AddScoped<IOllamaService, OllamaService>();
            services.AddSingleton<IConversationService, ConversationService>();

            // Queue
            services.AddHostedService<QueuedHostedService>();
            services.AddSingleton<IBackgroundTaskQueue>(_ =>
            {
                const int queueCapacity = 100;
                return new BackgroundTaskQueue(queueCapacity);
            });

            return services;
        }
    }
}