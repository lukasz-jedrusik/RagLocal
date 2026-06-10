using Rag.Services.Backend.Domain.Models;

namespace Rag.Services.Backend.Application.Interfaces.Repositories
{
    public interface IUserRepository
    {
        Task<User> GetByGoogleIdAsync(string googleId);
        Task<User> GetByIdAsync(int userId);
        Task<User> CreateAsync(User user);
        Task UpdateAsync(User user);
    }
}
