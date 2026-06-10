using Microsoft.EntityFrameworkCore;
using Rag.Services.Backend.Application.Interfaces.Repositories;
using Rag.Services.Backend.Domain.Models;
using Rag.Services.Backend.Infrastructure.Extensions.EfCore;

namespace Rag.Services.Backend.Infrastructure.Repositories
{
    public class MessageRepository : IMessageRepository
    {
        private readonly DataContext _context;

        public MessageRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<List<Message>> GetConversationMessagesAsync(int conversationId)
        {
            return await _context.Messages
                .Include(m => m.Sources)
                .Where(m => m.ConversationId == conversationId)
                .OrderBy(m => m.CreatedAt)
                .ToListAsync();
        }

        public async Task<Message> CreateAsync(Message message)
        {
            _context.Messages.Add(message);
            await _context.SaveChangesAsync();
            return message;
        }

        public async Task<Message> CreateWithSourcesAsync(Message message, List<MessageSource> sources)
        {
            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            // Add sources with the MessageId
            foreach (var source in sources)
            {
                source.MessageId = message.Id;
                _context.MessageSources.Add(source);
            }
            await _context.SaveChangesAsync();

            return message;
        }

        public async Task<int> GetMessageCountAsync(int conversationId)
        {
            return await _context.Messages
                .CountAsync(m => m.ConversationId == conversationId);
        }
    }
}
