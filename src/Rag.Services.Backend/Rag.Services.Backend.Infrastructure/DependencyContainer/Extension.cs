using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rag.Services.Backend.Application.Interfaces.Repositories;
using Rag.Services.Backend.Application.Interfaces.Services;
using Rag.Services.Backend.Application.Services;
using Rag.Services.Backend.Infrastructure.Extensions.EfCore;
using Rag.Services.Backend.Infrastructure.Repositories;
using Rag.Services.Backend.Infrastructure.Services;

namespace Rag.Services.Backend.Infrastructure.DependencyContainer
{
    public static class Extension
    {
        public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
        {
            // Repositories
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IConversationRepository, ConversationRepository>();
            services.AddScoped<IMessageRepository, MessageRepository>();

            // Services
            services.AddMemoryCache();
            services.AddHttpClient();
            services.AddScoped<IIdentityService, IdentityService>();
            services.AddScoped<IPdfLoaderService, PdfLoaderService>();
            services.AddScoped<IWordLoaderService, WordLoaderService>();
            services.AddScoped<IQdrantService, QdrantService>();
            services.AddScoped<IOllamaService, OllamaService>();
            services.AddSingleton<IConversationService, ConversationService>();

            // Queue
            services.AddHostedService<QueuedHostedService>();
            services.AddSingleton<IBackgroundTaskQueue>(static _ =>
            {
                const int queueCapacity = 100;
                return new BackgroundTaskQueue(queueCapacity);
            });

            return services;
        }
    }
}