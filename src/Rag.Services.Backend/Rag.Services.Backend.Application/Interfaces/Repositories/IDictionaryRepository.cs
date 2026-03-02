using Rag.Services.Backend.Domain.Models;

namespace Rag.Services.Backend.Application.Interfaces.Repositories
{
    public interface IDictionaryRepository<TEntity> where TEntity : DictionaryEntity
    {
        Task<List<TEntity>> GetAllAsync(DateTime? date);
        Task<TEntity> GetByIdAsync(int itemId);
        Task<List<TEntity>> GetByIdsAsync(List<int> itemsId);
        Task<List<TEntity>> GetBySentenceAsync(DateTime? date, string sentence);
        Task AddAsync(TEntity entity);
        Task<TEntity> GetAddedAsync(string name);
        Task DeleteAsync(int itemId);
        Task UpdateAsync(TEntity entity);
    }
}