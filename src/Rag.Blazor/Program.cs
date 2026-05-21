using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Rag.Blazor;
using Rag.Blazor.Services;
using Rag.Blazor.State;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");

builder.Services.AddScoped(sp =>
{
    var client = new HttpClient();
    return client;
});

builder.Services.AddScoped<StreamingClient>();

builder.Services.AddScoped<ChatApiClient>();

builder.Services.AddSingleton<ChatState>();

await builder.Build().RunAsync();