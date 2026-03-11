using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Rag.Services.Backend.Application.Interfaces.Services;
using Rag.Services.Backend.Domain.Models;

namespace Rag.Services.Backend.Infrastructure.Services
{
    public class QdrantService : IQdrantService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly string _collection = "docs";
        
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public QdrantService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient();
            _baseUrl = configuration["Qdrant:ServerUrl"];
        }

        public async Task InitAsync()
        {
            var createCollectionRequest = new
            {
                vectors = new
                {
                    size = 768,
                    distance = "Cosine"
                }
            };

            var json = JsonSerializer.Serialize(createCollectionRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PutAsync($"{_baseUrl}/collections/{_collection}", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    if (!errorContent.Contains("already exists"))
                    {
                        response.EnsureSuccessStatusCode();
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                // Handle connection errors (e.g., Qdrant not running)
                throw new InvalidOperationException("Failed to connect to Qdrant. Please ensure it is running and accessible.", ex);
            }
        }

        public async Task AddAsync(string text, string source, float[] vector)
        {
            var upsertRequest = new
            {
                points = new[]
                {
                    new
                    {
                        id = Guid.NewGuid().ToString(),
                        vector = vector,
                        payload = new
                        {
                            text = text,
                            source = source
                        }
                    }
                }
            };

            var json = JsonSerializer.Serialize(upsertRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"{_baseUrl}/collections/{_collection}/points", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Failed to add point to Qdrant. Status: {response.StatusCode}, Error: {errorContent}");
            }
        }

        public async Task<List<SearchResult>> SearchAsync(float[] query, int topK = 3)
        {
            var searchRequest = new
            {
                vector = query,
                limit = topK,
                with_payload = true
            };

            var json = JsonSerializer.Serialize(searchRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_baseUrl}/collections/{_collection}/points/search", content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var searchResponse = JsonSerializer.Deserialize<QdrantSearchResponse>(responseContent, JsonOptions);

            return searchResponse?.Result?.Select(hit => new SearchResult
            {
                Text = hit.Payload?.ContainsKey("text") == true ? hit.Payload["text"]?.ToString() ?? "" : "",
                Source = hit.Payload?.ContainsKey("source") == true ? hit.Payload["source"]?.ToString() ?? "" : "",
                Score = hit.Score
            })
                .ToList()
                ?? new List<SearchResult>();
        }
    }
}