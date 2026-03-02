using System.Text;
using System.Text.Json;
using Rag.Services.Backend.Application.Interfaces.Services;
using Rag.Services.Backend.Domain.Models;

namespace Rag.Services.Backend.Infrastructure.Extensions.Qdrant
{
    public class QdrantStore(IHttpClientFactory httpClientFactory)
        : IQdrantStore
    {
        private readonly HttpClient _httpClient = httpClientFactory.CreateClient();
        private readonly string _baseUrl = "http://localhost:6333";
        private readonly string _collection = "docs";

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
            catch (HttpRequestException)
            {
                // Handle connection errors (e.g., Qdrant not running)
                throw new Exception("Failed to connect to Qdrant. Please ensure it is running and accessible.");
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
                throw new Exception($"Failed to add point to Qdrant. Status: {response.StatusCode}, Error: {errorContent}");
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
            var searchResponse = JsonSerializer.Deserialize<QdrantSearchResponse>(responseContent, JsonConfig.DefaultOptions);

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