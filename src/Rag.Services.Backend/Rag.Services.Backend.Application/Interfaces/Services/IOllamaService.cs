using Rag.Services.Backend.Domain.Models;

namespace Rag.Services.Backend.Application.Interfaces.Services
{
    public interface IOllamaService
    {
        Task<string> AskAsync(string prompt);
        Task<string> AskWithHistoryAsync(string context, List<ConversationMessage> history, string currentQuestion);
        Task<float[]> CreateAsync(string text);
    }
}