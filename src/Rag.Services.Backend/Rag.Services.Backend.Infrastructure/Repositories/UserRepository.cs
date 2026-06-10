using Microsoft.EntityFrameworkCore;
using Rag.Services.Backend.Application.Interfaces.Repositories;
using Rag.Services.Backend.Domain.Models;
using Rag.Services.Backend.Infrastructure.Extensions.EfCore;

namespace Rag.Services.Backend.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly DataContext _context;

        public UserRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<User> GetByGoogleIdAsync(string googleId)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.GoogleId == googleId);
        }

        public async Task<User> GetByIdAsync(int userId)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<User> CreateAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task UpdateAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }
    }
}
