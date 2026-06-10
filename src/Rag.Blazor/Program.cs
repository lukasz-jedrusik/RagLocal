using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Rag.Blazor;
using Rag.Blazor.Models;
using Rag.Blazor.Services;
using Rag.Blazor.State;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");

// Configure API settings from appsettings.json
var apiSettings = builder.Configuration.GetSection("Api").Get<ApiSettings>() ?? new ApiSettings();
builder.Services.AddSingleton(apiSettings);

// Configure UI settings from appsettings.json
var uiSettings = builder.Configuration.GetSection("UI").Get<UiSettings>() ?? new UiSettings();
builder.Services.AddSingleton(uiSettings);

// Configure Google settings from appsettings.json
var googleSettings = builder.Configuration.GetSection("Google").Get<GoogleSettings>() ?? new GoogleSettings();
builder.Services.AddSingleton(googleSettings);

// Register AuthService
builder.Services.AddScoped<AuthService>();

// Register AuthenticatedHttpClient
builder.Services.AddScoped<AuthenticatedHttpClient>();

builder.Services.AddScoped(sp =>
{
    var authenticatedClient = sp.GetRequiredService<AuthenticatedHttpClient>();
    return authenticatedClient.HttpClient;
});

builder.Services.AddScoped<StreamingClient>();

builder.Services.AddScoped<ChatApiClient>();

builder.Services.AddSingleton<ChatState>();

await builder.Build().RunAsync();