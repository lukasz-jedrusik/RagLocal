using Rag.Services.Backend.Domain.Models;

namespace Rag.Services.Backend.Application.Interfaces.Repositories
{
    public interface IConversationRepository
    {
        Task<Conversation> GetByConversationIdAsync(string conversationId, int userId);
        Task<List<Conversation>> GetUserConversationsAsync(int userId);
        Task<Conversation> CreateAsync(Conversation conversation);
        Task UpdateAsync(Conversation conversation);
        Task DeleteAsync(int conversationId);
    }
}
