namespace Rag.Services.Backend.Application.Interfaces.Services
{
    public interface IOllamaEmbedding
    {
        Task<float[]> CreateAsync(string text);
    }
}