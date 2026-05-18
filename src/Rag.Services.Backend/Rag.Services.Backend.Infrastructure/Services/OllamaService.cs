using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Rag.Services.Backend.Application.Interfaces.Services;
using Rag.Services.Backend.Domain.Models;

namespace Rag.Services.Backend.Infrastructure.Services
{
    public class OllamaService : IOllamaService
    {
        private readonly HttpClient _http;

        public OllamaService(IConfiguration configuration)
        {
            var serverUrl = configuration["Ollama:ServerUrl"];
            _http = new HttpClient
            {
                BaseAddress = new Uri(serverUrl)
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

        public async Task<string> AskWithHistoryAsync(string context, List<ConversationMessage> history, string currentQuestion)
        {
            var messages = new List<object>();

            // Add system message with context
            messages.Add(new { role = "system", content = $"Answer ONLY using this context:\n\n{context}" });

            // Add conversation history
            foreach (var msg in history)
            {
                messages.Add(new { role = msg.Role, content = msg.Content });
            }

            // Add current question
            messages.Add(new { role = "user", content = currentQuestion });

            // Create request body
            var req = new
            {
                model = "llama3",
                messages = messages,
                stream = false
            };

            // Send request to Ollama API
            var res = await _http.PostAsJsonAsync("/api/chat", req);

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

        public async IAsyncEnumerable<string> AskWithHistoryStreamAsync(
            string context, 
            List<ConversationMessage> history,
            string currentQuestion,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var messages = new List<object>();

            // Add system message with context
            messages.Add(new { role = "system", content = $"Answer ONLY using this context:\n\n{context}" });

            // Add conversation history
            foreach (var msg in history)
            {
                messages.Add(new { role = msg.Role, content = msg.Content });
            }

            // Add current question
            messages.Add(new { role = "user", content = currentQuestion });

            // Create request body with streaming enabled
            var req = new
            {
                model = "llama3",
                messages = messages,
                stream = true
            };

            // Send request to Ollama API
            var response = await _http.PostAsJsonAsync("/api/chat", req, cancellationToken);
            response.EnsureSuccessStatusCode();

            // Read the stream line by line
            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var reader = new StreamReader(stream);

            string line;
            while ((line = await reader.ReadLineAsync(cancellationToken)) != null && !cancellationToken.IsCancellationRequested)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                // Parse JSON line
                using var doc = JsonDocument.Parse(line);
                var root = doc.RootElement;

                // Check if the response is done
                if (root.TryGetProperty("done", out var done) && done.GetBoolean())
                {
                    break;
                }

                // Extract the message content (token)
                if (root.TryGetProperty("message", out var message) &&
                    message.TryGetProperty("content", out var content))
                {
                    var token = content.GetString();
                    if (!string.IsNullOrEmpty(token))
                    {
                        yield return token;
                    }
                }
            }
        }
    }
}