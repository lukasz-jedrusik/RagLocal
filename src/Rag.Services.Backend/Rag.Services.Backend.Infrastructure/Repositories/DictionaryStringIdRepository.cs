using Rag.Services.Backend.Application.Interfaces.Repositories;
using Rag.Services.Backend.Domain.Models;
using Rag.Services.Backend.Infrastructure.Extensions.EfCore;
using Microsoft.EntityFrameworkCore;

namespace Rag.Services.Backend.Infrastructure.Repositories
{
    public class DictionaryStringIdRepository<TEntity>(
        DataContext context) : IDictionaryStringIdRepository<TEntity> where TEntity : DictionaryStringIdEntity
    {
        protected readonly DataContext _context = context;

        public Task<List<TEntity>> GetAllAsync(DateTime? date)
        {
            if (date == null)
            {
                date = DateTime.UtcNow;
            }

            return _context.Set<TEntity>()
                .Where(x => x.CreateDate <= date && (x.DeleteDate == null || x.DeleteDate >= date))
                .ToListAsync();
        }

        public Task<TEntity> GetByIdAsync(string itemId) =>
            _context.Set<TEntity>().FirstOrDefaultAsync(x => x.Id == itemId);

        public async Task AddAsync(TEntity entity)
        {
            entity.CreateDate = DateTime.UtcNow;
            _context.Set<TEntity>().Add(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(string itemId)
        {
            var itemToDelete = await _context.Set<TEntity>().Where(x => x.Id == itemId).FirstOrDefaultAsync();

            if (itemToDelete == null)
            {
                return;
            }

            itemToDelete.DeleteDate = DateTime.UtcNow;

            _context.Entry(itemToDelete).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public virtual async Task UpdateAsync(TEntity entity)
        {
            var itemToUpdate = await _context.Set<TEntity>().Where(x => x.Id == entity.Id).FirstOrDefaultAsync();

            if (itemToUpdate == null)
            {
                return;
            }

            itemToUpdate.Name = entity.Name;

            _context.Entry(itemToUpdate).State = EntityState.Modified;

            await _context.SaveChangesAsync();
        }

        public Task<List<TEntity>> GetByIdsAsync(List<string> itemsId) =>
            _context.Set<TEntity>().Where(x => itemsId.Contains(x.Id)).ToListAsync();

        public Task<List<TEntity>> GetBySentenceAsync(DateTime? date, string sentence)
        {
            if (date == null)
            {
                date = DateTime.UtcNow;
            }

            return _context.Set<TEntity>()
                .Where(x => x.CreateDate <= date && (x.DeleteDate == null || x.DeleteDate >= date))
                .Where(x => x.Name.Contains(sentence, StringComparison.OrdinalIgnoreCase))
                .ToListAsync();
        }

        public Task<TEntity> GetAddedAsync(string itemId) =>
            _context.Set<TEntity>().Where(x => x.Id == itemId).LastOrDefaultAsync();
    }
}