using System.Text;
using System.Text.Json;
using MediatR;
using Microsoft.AspNetCore.Http;
using Rag.Services.Backend.Application.DataTransferObjects.Streaming;
using Rag.Services.Backend.Application.Helpers;
using Rag.Services.Backend.Application.Interfaces.Services;
using Rag.Services.Backend.Domain.Models;

namespace Rag.Services.Backend.Application.Queries.AskQuestionStream
{
    public class AskQuestionStreamQueryHandler(
        IQdrantService vectorStore,
        IOllamaService ollama,
        IConversationService conversationService) : IRequestHandler<AskQuestionStreamQuery>
    {
        private readonly IQdrantService _vectorStore = vectorStore;
        private readonly IOllamaService _ollamaService = ollama;
        private readonly IConversationService _conversationService = conversationService;

        public async Task Handle(
            AskQuestionStreamQuery request,
            CancellationToken cancellationToken)
        {
            // Configure response for Server-Sent Events
            request.Response.ContentType = "text/event-stream";
            request.Response.Headers.CacheControl = "no-cache";
            // Note: Connection header is invalid for HTTP/2 and HTTP/3 - removed

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

            // Send meta event with conversation ID
            var metaDto = new StreamMetaDto { ConversationId = conversationId };
            var metaMessage = $"event: meta\ndata: {JsonSerializer.Serialize(metaDto)}\n\n";
            await request.Response.WriteAsync(metaMessage, request.CancellationToken);
            await request.Response.Body.FlushAsync(request.CancellationToken);

            // Stream the answer with token buffering
            StringBuilder fullAnswerBuilder = new();
            var tokenBuffer = new StreamTokenBuffer();

            await foreach (string token in _ollamaService.AskWithHistoryStreamAsync(context, history, request.Question, cancellationToken))
            {
                fullAnswerBuilder.Append(token);

                // Process token through buffer to avoid splitting words
                foreach (var bufferedToken in tokenBuffer.ProcessToken(token))
                {
                    var tokenDto = new StreamTokenDto { Token = bufferedToken };
                    var tokenMessage = $"data: {JsonSerializer.Serialize(tokenDto)}\n";
                    await request.Response.WriteAsync(tokenMessage, request.CancellationToken);
                    await request.Response.Body.FlushAsync(request.CancellationToken);
                }
            }

            // Flush any remaining buffered content
            var remainingToken = tokenBuffer.Flush();
            if (!string.IsNullOrEmpty(remainingToken))
            {
                var tokenDto = new StreamTokenDto { Token = remainingToken };
                var tokenMessage = $"data: {JsonSerializer.Serialize(tokenDto)}\n";
                await request.Response.WriteAsync(tokenMessage, request.CancellationToken);
                await request.Response.Body.FlushAsync(request.CancellationToken);
            }

            // Send citations event with sources
            var citationsDto = new StreamCitationsDto
            {
                Sources = hits.Select((hit, index) => new StreamSourceDto
                {
                    Id = index + 1,
                    Title = hit.Source,
                    Url = hit.Source,
                    Excerpt = hit.Text.Length > 200 ? hit.Text.Substring(0, 200) + "..." : hit.Text,
                    Score = hit.Score
                }).ToList()
            };

            // Only send citations if there are sources to include
            var citationsMessage = $"event: citations\ndata: {JsonSerializer.Serialize(citationsDto)}\n\n";
            await request.Response.WriteAsync(citationsMessage, request.CancellationToken);
            await request.Response.Body.FlushAsync(request.CancellationToken);

            // Send done marker
            var doneMessage = "data: [DONE]\n\n";
            await request.Response.WriteAsync(doneMessage, request.CancellationToken);
            await request.Response.Body.FlushAsync(request.CancellationToken);

            // Add assistant answer to conversation history after streaming is complete
            await _conversationService.AddMessageAsync(conversationId, "assistant", fullAnswerBuilder.ToString());
        }
    }
}
