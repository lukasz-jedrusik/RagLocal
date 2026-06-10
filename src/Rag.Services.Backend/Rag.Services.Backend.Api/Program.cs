using System.Text.Json.Serialization;
using NLog.Web;
using Rag.Services.Backend.Api.Endpoints;
using Rag.Services.Backend.Api.Middleware;
using Rag.Services.Backend.Application.Mappings;
using Rag.Services.Backend.Infrastructure.DependencyContainer;
using Rag.Services.Backend.Infrastructure.Extensions.EfCore;
using Rag.Services.Backend.Infrastructure.Extensions.GmailAuth;
using Rag.Services.Backend.Infrastructure.Extensions.MediatR;
using Rag.Services.Backend.Infrastructure.Extensions.Swagger;

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
    .AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            policy.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        });
    })
    .AddEfCore(builder.Configuration)
    .AddEndpointsApiExplorer()
    .AddSwagger()
    .AddGmailAuthorization(builder.Configuration)
    .AddMediatR()
    .AddApplication(builder.Configuration);

// Add authentication and authorization
// Authentication is configured in GmailAuth extension

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
    .UseAuthentication()
    .UseAuthorization();

// Use auto migrations applying
app.ApplyMigrations();

// Use controllers in pipieline
app.MapControllers();

// Use healtheckecks in pipeline
app.MapHealthChecks("/health");

// Map endpoints
app.MapEndpoints();

// Run app
await app.RunAsync();