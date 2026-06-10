using Microsoft.EntityFrameworkCore;
using Rag.Services.Backend.Application.Interfaces.Repositories;
using Rag.Services.Backend.Domain.Models;
using Rag.Services.Backend.Infrastructure.Extensions.EfCore;

namespace Rag.Services.Backend.Infrastructure.Repositories
{
    public class ConversationRepository : IConversationRepository
    {
        private readonly DataContext _context;

        public ConversationRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<Conversation> GetByConversationIdAsync(string conversationId, int userId)
        {
            return await _context.Conversations
                .FirstOrDefaultAsync(c => c.ConversationId == conversationId && c.UserId == userId);
        }

        public async Task<List<Conversation>> GetUserConversationsAsync(int userId)
        {
            return await _context.Conversations
                .Where(c => c.UserId == userId && c.IsActive)
                .OrderByDescending(c => c.UpdatedAt)
                .ToListAsync();
        }

        public async Task<Conversation> CreateAsync(Conversation conversation)
        {
            _context.Conversations.Add(conversation);
            await _context.SaveChangesAsync();
            return conversation;
        }

        public async Task UpdateAsync(Conversation conversation)
        {
            _context.Conversations.Update(conversation);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int conversationId)
        {
            var conversation = await _context.Conversations.FindAsync(conversationId);
            if (conversation != null)
            {
                conversation.IsActive = false;
                await _context.SaveChangesAsync();
            }
        }
    }
}
