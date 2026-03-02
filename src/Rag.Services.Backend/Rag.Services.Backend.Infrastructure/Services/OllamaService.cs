using System.Net.Http.Json;
using Rag.Services.Backend.Application.Interfaces.Services;
using Rag.Services.Backend.Domain.Models;

namespace Rag.Services.Backend.Infrastructure.Services
{
    public class OllamaService : IOllamaService
    {
        private readonly HttpClient _http;

        public OllamaService()
        {
            _http = new HttpClient
            {
                BaseAddress = new Uri("http://localhost:11434")
            };
        }

        public async Task<string> AskAsync(string prompt)
        {
            // Create request body
            var req = new
            {
                model = "llama3",
                messages = new[]
                {
                    new { role = "user", content = prompt }
                },
                stream = false
            };

            // Send request to Ollama API
            var res = await _http.PostAsJsonAsync(
                "/api/chat",
                req
            );

            // Get response content
            var json = await res.Content.ReadFromJsonAsync<OllamaResponse>();

            // Return response message content
            return json.Message.Content;
        }

        public async Task<float[]> CreateAsync(string text)
        {
            var response = await _http.PostAsJsonAsync(
                "/api/embeddings",
                new EmbeddingRequest
                {
                    Model = "nomic-embed-text",
                    Prompt = text
                });

            response.EnsureSuccessStatusCode();

            var result = await response.Content
                .ReadFromJsonAsync<EmbeddingResponse>();

            return result!.Embedding;
        }
    }
}