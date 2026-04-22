using System.Text.Json.Serialization;
using Rag.Services.Backend.Api.Middleware;
using Rag.Services.Backend.Api.Endpoints;
using Rag.Services.Backend.Application.Mappings;
using Rag.Services.Backend.Infrastructure.DependencyContainer;
using Rag.Services.Backend.Infrastructure.Extensions.KeycloakAuth;
using Rag.Services.Backend.Infrastructure.Extensions.MediatR;
using Rag.Services.Backend.Infrastructure.Extensions.Swagger;
using Mapster;
using NLog.Web;

// Create builder
var builder = WebApplication.CreateBuilder(args);

// Add controllers to services
builder.Services
    .AddControllers(x => x.AllowEmptyInputInBodyModelBinding = true)
    .AddJsonOptions(x => x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// Add Nlog
builder.Logging.ClearProviders();
builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));
builder.Logging.AddConsole();
builder.Host.UseNLog(new NLogAspNetCoreOptions() { RemoveLoggerFactoryFilter = false });

// Add services
builder.Services
    .AddCors()
    .AddEndpointsApiExplorer()
    .AddSwagger()
    .AddKeycloakAuthorization(builder.Configuration)
    .AddMediatR()
    .AddApplication();

// Configure Mapster
MapsterConfig.Configure();

// Add healtchecks endpoints
builder.Services.AddHealthChecks();

// Create app
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Use Cors, Middleware, https redirection, authorization in pipieline
app.UseCors()
    .UseMiddleware<ErrorHandlerMiddleware>()
    .UseHttpsRedirection()
    .UseAuthorization();

// Use controllers in pipieline
app.MapControllers();

// Use healtheckecks in pipeline
app.MapHealthChecks("/health");

// Map endpoints
app.MapEndpoints();

// Run app
await app.RunAsync();