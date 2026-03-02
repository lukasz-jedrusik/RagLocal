using MediatR;
using Rag.Services.Backend.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Rag.Services.Backend.Infrastructure.Extensions.MediatR
{
    public static class Extension
    {
        public static IServiceCollection AddMediatR(this IServiceCollection services)
        {
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(BackgroundTaskQueue).Assembly));
            return services;
        }
    }
}