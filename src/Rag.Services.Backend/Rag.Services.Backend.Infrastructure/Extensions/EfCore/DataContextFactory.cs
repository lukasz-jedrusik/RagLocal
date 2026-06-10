using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Rag.Services.Backend.Infrastructure.Extensions.EfCore
{
    /// <summary>
    /// Design-time factory for EF Core migrations.
    /// This class is only used by EF Core tooling (dotnet ef migrations add/update).
    /// </summary>
    public class DataContextFactory : IDesignTimeDbContextFactory<DataContext>
    {
        public DataContext CreateDbContext(string[] args)
        {
            // Build configuration from appsettings.json
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<DataContext>();
            optionsBuilder.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));

            return new DataContext(optionsBuilder.Options);
        }
    }
}
