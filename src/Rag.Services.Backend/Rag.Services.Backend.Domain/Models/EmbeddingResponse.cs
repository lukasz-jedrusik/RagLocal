using System.Text.Json.Serialization;

namespace Rag.Services.Backend.Domain.Models
{
    public class EmbeddingResponse
    {
        [JsonPropertyName("embedding")]
        public float[] Embedding { get; set; } = default!;
    }
}