using Microsoft.Extensions.Caching.Memory;
using Rag.Services.Backend.Application.Interfaces.Services;
using Rag.Services.Backend.Domain.Models;

namespace Rag.Services.Backend.Infrastructure.Services
{
    public class ConversationService : IConversationService
    {
        private readonly IMemoryCache _cache;
        private static readonly TimeSpan ConversationTimeout = TimeSpan.FromHours(1);

        public ConversationService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public string CreateConversation()
        {
            var conversationId = Guid.NewGuid().ToString();
            var messages = new List<ConversationMessage>();
            
            _cache.Set(conversationId, messages, new MemoryCacheEntryOptions
            {
                SlidingExpiration = ConversationTimeout
            });

            return conversationId;
        }

        public Task AddMessageAsync(string conversationId, string role, string content)
        {
            if (!_cache.TryGetValue(conversationId, out object messagesObj) || messagesObj is not List<ConversationMessage> messages)
            {
                throw new KeyNotFoundException($"Conversation with ID '{conversationId}' not found.");
            }

            messages.Add(new ConversationMessage
            {
                Role = role,
                Content = content
            });

            // Refresh sliding expiration
            _cache.Set(conversationId, messages, new MemoryCacheEntryOptions
            {
                SlidingExpiration = ConversationTimeout
            });

            return Task.CompletedTask;
        }

        public Task<List<ConversationMessage>> GetHistoryAsync(string conversationId)
        {
            if (!_cache.TryGetValue(conversationId, out object messagesObj) || messagesObj is not List<ConversationMessage> messages)
            {
                throw new KeyNotFoundException($"Conversation with ID '{conversationId}' not found.");
            }

            // Refresh sliding expiration
            _cache.Set(conversationId, messages, new MemoryCacheEntryOptions
            {
                SlidingExpiration = ConversationTimeout
            });

            return Task.FromResult(messages);
        }

        public Task ClearConversationAsync(string conversationId)
        {
            _cache.Remove(conversationId);
            return Task.CompletedTask;
        }
    }
}
