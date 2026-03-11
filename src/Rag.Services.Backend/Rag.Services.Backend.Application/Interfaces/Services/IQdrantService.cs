using Rag.Services.Backend.Domain.Models;

namespace Rag.Services.Backend.Application.Interfaces.Services
{
    public interface IQdrantService
    {
        Task InitAsync();
        Task AddAsync(string text, string source, float[] vector);
        Task<List<SearchResult>> SearchAsync(float[] query, int topK = 3);
    }
}