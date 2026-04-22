using Rag.Services.Backend.Domain.Models;

namespace Rag.Services.Backend.Application.Interfaces.Services
{
    public interface IConversationService
    {
        string CreateConversation();
        Task AddMessageAsync(string conversationId, string role, string content);
        Task<List<ConversationMessage>> GetHistoryAsync(string conversationId);
        Task ClearConversationAsync(string conversationId);
    }
}
