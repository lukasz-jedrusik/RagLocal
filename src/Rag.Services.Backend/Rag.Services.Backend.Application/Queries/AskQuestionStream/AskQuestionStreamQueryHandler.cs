using System.Runtime.CompilerServices;
using System.Text;
using MediatR;
using Rag.Services.Backend.Application.Interfaces.Services;
using Rag.Services.Backend.Domain.Models;

namespace Rag.Services.Backend.Application.Queries.AskQuestionStream
{
    public class AskQuestionStreamQueryHandler(
        IQdrantService vectorStore,
        IOllamaService ollama,
        IConversationService conversationService) : IStreamRequestHandler<AskQuestionStreamQuery, string>
    {
        private readonly IQdrantService _vectorStore = vectorStore;
        private readonly IOllamaService _ollamaService = ollama;
        private readonly IConversationService _conversationService = conversationService;

        public async IAsyncEnumerable<string> Handle(
            AskQuestionStreamQuery request,
            [EnumeratorCancellation] CancellationToken cancellationToken)
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
                    history = [];
                }
            }
            else
            {
                // Create new conversation
                conversationId = _conversationService.CreateConversation();
                history = [];
            }

            // Get embeddings and search for context
            float[] qVec = await _ollamaService.CreateAsync(request.Question);
            List<SearchResult> hits = await _vectorStore.SearchAsync(qVec);
            string context = string.Join("\n\n", hits.Select(static h => h.Text));

            // Add user question to conversation history first
            await _conversationService.AddMessageAsync(conversationId, "user", request.Question);

            // Yield conversation ID as first message (prefixed with a marker)
            yield return $"[CONVERSATION_ID]{conversationId}";

            // Stream the answer
            StringBuilder fullAnswerBuilder = new StringBuilder();
            await foreach (string token in _ollamaService.AskWithHistoryStreamAsync(context, history, request.Question, cancellationToken))
            {
                _ = fullAnswerBuilder.Append(token);
                yield return token;
            }

            // Add assistant answer to conversation history after streaming is complete
            await _conversationService.AddMessageAsync(conversationId, "assistant", fullAnswerBuilder.ToString());
        }
    }
}
