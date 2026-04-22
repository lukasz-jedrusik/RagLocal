using MediatR;
using Rag.Services.Backend.Application.DataTransferObjects;
using Rag.Services.Backend.Application.Interfaces.Services;
using Rag.Services.Backend.Domain.Models;

namespace Rag.Services.Backend.Application.Queries.AskQuestion
{
    public class AskQuestionQueryHandler(
        IQdrantService vectorStore,
        IOllamaService ollama,
        IConversationService conversationService) : IRequestHandler<AskQuestionQuery, AskResponseDto>
    {
        private readonly IQdrantService _vectorStore = vectorStore;
        private readonly IOllamaService _ollamaService = ollama;
        private readonly IConversationService _conversationService = conversationService;

        public async Task<AskResponseDto> Handle(
            AskQuestionQuery request,
            CancellationToken cancellationToken)
        {
            // Create or retrieve conversation
            string conversationId;
            List<ConversationMessage> history;

            if (!string.IsNullOrEmpty(request.ConversationId))
            {
                try
                {
                    conversationId = request.ConversationId;
                    history = await _conversationService.GetHistoryAsync(conversationId);
                }
                catch (KeyNotFoundException)
                {
                    // If conversation not found, create new one
                    conversationId = _conversationService.CreateConversation();
                    history = new List<ConversationMessage>();
                }
            }
            else
            {
                // Create new conversation
                conversationId = _conversationService.CreateConversation();
                history = new List<ConversationMessage>();
            }

            // Get embeddings and search for context
            var qVec = await _ollamaService.CreateAsync(request.Question);
            var hits = await _vectorStore.SearchAsync(qVec);
            var context = string.Join("\n\n", hits.Select(h => h.Text));

            // Get answer with conversation history
            string answer = await _ollamaService.AskWithHistoryAsync(context, history, request.Question);

            // Add user question and assistant answer to conversation history
            await _conversationService.AddMessageAsync(conversationId, "user", request.Question);
            await _conversationService.AddMessageAsync(conversationId, "assistant", answer);

            return new AskResponseDto
            {
                Answer = answer,
                ConversationId = conversationId
            };
        }
    }
}