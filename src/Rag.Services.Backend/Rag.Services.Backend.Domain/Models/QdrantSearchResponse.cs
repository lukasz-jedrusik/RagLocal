using System.Text.Json.Serialization;

namespace Rag.Services.Backend.Domain.Models
{
    public class QdrantSearchResponse
    {
        [JsonPropertyName("result")]
        public QdrantSearchResult[] Result { get; set; }
    }
}