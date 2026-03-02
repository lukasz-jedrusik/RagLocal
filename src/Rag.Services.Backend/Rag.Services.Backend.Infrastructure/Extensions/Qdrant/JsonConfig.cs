using System.Text.Json;

namespace Rag.Services.Backend.Infrastructure.Extensions.Qdrant
{
    public static class JsonConfig
    {
        public static readonly JsonSerializerOptions DefaultOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };
    }
}