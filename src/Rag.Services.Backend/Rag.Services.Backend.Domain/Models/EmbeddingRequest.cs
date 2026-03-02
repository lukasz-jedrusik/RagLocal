using System.Text.Json.Serialization;

namespace Rag.Services.Backend.Domain.Models
{
    public class EmbeddingRequest
    {
        [JsonPropertyName("model")]
        public string Model { get; set; } = default!;

        [JsonPropertyName("prompt")]
        public string Prompt { get; set; } = default!;
    }
}