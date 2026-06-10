using Rag.Services.Backend.Domain.Models;

namespace Rag.Services.Backend.Application.Interfaces.Repositories
{
    public interface IMessageRepository
    {
        Task<List<Message>> GetConversationMessagesAsync(int conversationId);
        Task<Message> CreateAsync(Message message);
        Task<Message> CreateWithSourcesAsync(Message message, List<MessageSource> sources);
        Task<int> GetMessageCountAsync(int conversationId);
    }
}
