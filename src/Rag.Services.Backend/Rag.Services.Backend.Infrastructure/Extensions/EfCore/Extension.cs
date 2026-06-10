using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Rag.Services.Backend.Infrastructure.Extensions.EfCore
{
    public static class Extension
    {
        public static IServiceCollection AddEfCore(this IServiceCollection services, IConfiguration configuration)
        {
            _ = services.AddDbContext<DataContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            return services;
        }
    }
}