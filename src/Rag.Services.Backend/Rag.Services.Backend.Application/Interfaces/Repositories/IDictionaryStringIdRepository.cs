using Rag.Services.Backend.Domain.Models;

namespace Rag.Services.Backend.Application.Interfaces.Repositories
{
    public interface IDictionaryStringIdRepository<TEntity> where TEntity : DictionaryStringIdEntity
    {
        Task<List<TEntity>> GetAllAsync(DateTime? date);
        Task<TEntity> GetByIdAsync(string itemId);
        Task<List<TEntity>> GetByIdsAsync(List<string> itemsId);
        Task<List<TEntity>> GetBySentenceAsync(DateTime? date, string sentence);
        Task AddAsync(TEntity entity);
        Task<TEntity> GetAddedAsync(string itemId);
        Task DeleteAsync(string itemId);
        Task UpdateAsync(TEntity entity);
    }
}