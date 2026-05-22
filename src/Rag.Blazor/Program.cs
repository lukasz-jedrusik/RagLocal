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

builder.Services.AddScoped(sp =>
{
    var client = new HttpClient();
    return client;
});

builder.Services.AddScoped<StreamingClient>();

builder.Services.AddScoped<ChatApiClient>();

builder.Services.AddSingleton<ChatState>();

await builder.Build().RunAsync();