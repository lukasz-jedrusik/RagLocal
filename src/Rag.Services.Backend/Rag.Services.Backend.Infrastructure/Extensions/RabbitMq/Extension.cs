using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;

namespace Rag.Services.Backend.Infrastructure.Extensions.RabbitMq
{
    public static class Extension
    {
        public static IServiceCollection AddRabbitMq(this IServiceCollection services, IConfiguration configuration)
        {
            var factory = new ConnectionFactory { HostName = configuration["RabbitMq:Hostname"] };
            var connection = factory.CreateConnection();

            services.AddSingleton(connection);

            return services;
        }
    }
}