using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace Rag.Services.Backend.Infrastructure.Extensions.Swagger
{
    public static class Extension
    {
        public static IServiceCollection AddSwagger(this IServiceCollection services)
        {
            _ = services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc(
                    "v1",
                    new OpenApiInfo
                        {
                            Title = "Rag.Services.Backend API",
                            Description = "ASP.NET Core Rag.Services.Backend API",
                            Version = "v1"
                        });

                // Configure for OpenAPI 3.0 compliance  
                c.DescribeAllParametersInCamelCase();
                c.UseInlineDefinitionsForEnums();
                
                // Enable annotations for better API documentation
                c.EnableAnnotations();
                
                // Support for Minimal APIs
                c.TagActionsBy(api => new[] { api.GroupName ?? "Default" });
                c.DocInclusionPredicate((name, api) => true);
            });

            return services;
        }
    }
}