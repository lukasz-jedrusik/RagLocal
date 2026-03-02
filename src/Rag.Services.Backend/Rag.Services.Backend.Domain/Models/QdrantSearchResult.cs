using System.Text.Json.Serialization;

namespace Rag.Services.Backend.Domain.Models
{
    public class QdrantSearchResult
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("score")]
        public float Score { get; set; }

        [JsonPropertyName("payload")]
        public Dictionary<string, object> Payload { get; set; }
    }
}