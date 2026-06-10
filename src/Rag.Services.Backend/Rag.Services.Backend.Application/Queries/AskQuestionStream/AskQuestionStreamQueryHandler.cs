using System.Text;
using System.Text.Json;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Rag.Services.Backend.Application.DataTransferObjects.Streaming;
using Rag.Services.Backend.Application.Helpers;
using Rag.Services.Backend.Application.Interfaces.Repositories;
using Rag.Services.Backend.Application.Interfaces.Services;
using Rag.Services.Backend.Domain.Models;

namespace Rag.Services.Backend.Application.Queries.AskQuestionStream
{
    public class AskQuestionStreamQueryHandler(
        IQdrantService vectorStore,
        IOllamaService ollama,
        IConversationService conversationService,
        IConversationRepository conversationRepository,
        IMessageRepository messageRepository,
        IBackgroundTaskQueue backgroundTaskQueue,
        IServiceScopeFactory serviceScopeFactory,
        ILogger<AskQuestionStreamQueryHandler> logger) : IRequestHandler<AskQuestionStreamQuery>
    {
        private readonly IQdrantService _vectorStore = vectorStore;
        private readonly IOllamaService _ollamaService = ollama;
        private readonly IConversationService _conversationService = conversationService;
        private readonly IConversationRepository _conversationRepository = conversationRepository;
        private readonly IMessageRepository _messageRepository = messageRepository;
        private readonly IBackgroundTaskQueue _backgroundTaskQueue = backgroundTaskQueue;
        private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;
        private readonly ILogger<AskQuestionStreamQueryHandler> _logger = logger;

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
            Conversation dbConversation = null;

            if (!string.IsNullOrEmpty(request.ConversationId))
            {
                // Try to get from database first
                dbConversation = await _conversationRepository.GetByConversationIdAsync(request.ConversationId, request.UserId);

                if (dbConversation != null)
                {
                    conversationId = request.ConversationId;

                    // Load history from database
                    var dbMessages = await _messageRepository.GetConversationMessagesAsync(dbConversation.Id);
                    history = dbMessages.ConvertAll(m => new ConversationMessage
                    {
                        Role = m.Role,
                        Content = m.Content
                    });

                    // Also sync with in-memory conversation service
                    try
                    {
                        _ = await _conversationService.GetHistoryAsync(conversationId);
                    }
                    catch (KeyNotFoundException)
                    {
                        // Create in memory if not exists
                        _ = _conversationService.CreateConversation(conversationId);
                        foreach (var msg in history)
                            await _conversationService.AddMessageAsync(conversationId, msg.Role, msg.Content);
                    }
                }
                else
                {
                    // Conversation not found in DB, create new
                    conversationId = _conversationService.CreateConversation();
                    history = [];

                    // Create in database
                    dbConversation = new Conversation
                    {
                        ConversationId = conversationId,
                        UserId = request.UserId,
                        Title = request.Question.Length > 100 ? request.Question[..100] + "..." : request.Question,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        IsActive = true
                    };
                    dbConversation = await _conversationRepository.CreateAsync(dbConversation);
                }
            }
            else
            {
                // Create new conversation
                conversationId = _conversationService.CreateConversation();
                history = [];

                // Create in database
                dbConversation = new Conversation
                {
                    ConversationId = conversationId,
                    UserId = request.UserId,
                    Title = request.Question.Length > 100 ? request.Question[..100] + "..." : request.Question,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsActive = true
                };
                dbConversation = await _conversationRepository.CreateAsync(dbConversation);
            }

            // Get embeddings and search for context
            var qVec = await _ollamaService.CreateAsync(request.Question);
            var hits = await _vectorStore.SearchAsync(qVec);
            var context = string.Join("\n\n", hits.Select(static h => h.Text));

            // Add user question to conversation history first (in-memory)
            await _conversationService.AddMessageAsync(conversationId, "user", request.Question);

            // Send meta event with conversation ID
            StreamMetaDto metaDto = new() { ConversationId = conversationId };
            var metaMessage = $"event: meta\ndata: {JsonSerializer.Serialize(metaDto)}\n\n";
            await request.Response.WriteAsync(metaMessage, request.CancellationToken);
            await request.Response.Body.FlushAsync(request.CancellationToken);

            // Stream the answer with token buffering
            StringBuilder fullAnswerBuilder = new();
            StreamTokenBuffer tokenBuffer = new();

            await foreach (var token in _ollamaService.AskWithHistoryStreamAsync(context, history, request.Question, cancellationToken))
            {
                _ = fullAnswerBuilder.Append(token);

                // Process token through buffer to avoid splitting words
                foreach (var bufferedToken in tokenBuffer.ProcessToken(token))
                {
                    StreamTokenDto tokenDto = new() { Token = bufferedToken };
                    var tokenMessage = $"data: {JsonSerializer.Serialize(tokenDto)}\n";
                    await request.Response.WriteAsync(tokenMessage, request.CancellationToken);
                    await request.Response.Body.FlushAsync(request.CancellationToken);
                }
            }

            // Flush any remaining buffered content
            var remainingToken = tokenBuffer.Flush();
            if (!string.IsNullOrEmpty(remainingToken))
            {
                StreamTokenDto tokenDto = new() { Token = remainingToken };
                var tokenMessage = $"data: {JsonSerializer.Serialize(tokenDto)}\n";
                await request.Response.WriteAsync(tokenMessage, request.CancellationToken);
                await request.Response.Body.FlushAsync(request.CancellationToken);
            }

            // Send citations event with sources
            StreamCitationsDto citationsDto = new()
            {
                Sources = [.. hits.Select((hit, index) => new StreamSourceDto
                {
                    Id = index + 1,
                    Title = hit.Source,
                    Url = hit.Source,
                    Excerpt = hit.Text.Length > 200 ? hit.Text[..200] + "..." : hit.Text,
                    Score = hit.Score
                })]
            };

            // Only send citations if there are sources to include
            var citationsMessage = $"event: citations\ndata: {JsonSerializer.Serialize(citationsDto)}\n\n";
            await request.Response.WriteAsync(citationsMessage, request.CancellationToken);
            await request.Response.Body.FlushAsync(request.CancellationToken);

            // Send done marker
            const string doneMessage = "data: [DONE]\n\n";
            await request.Response.WriteAsync(doneMessage, request.CancellationToken);
            await request.Response.Body.FlushAsync(request.CancellationToken);

            // Add assistant answer to conversation history after streaming is complete (in-memory)
            var fullAnswer = fullAnswerBuilder.ToString();
            await _conversationService.AddMessageAsync(conversationId, "assistant", fullAnswer);

            // Capture variables needed for background save
            var dbConversationId = dbConversation.Id;
            var conversationGuid = conversationId;
            var userQuestion = request.Question;
            var assistantAnswer = fullAnswer;
            var userId = request.UserId;

            // Capture sources for database save
            var sources = hits.Select((hit, index) => new
            {
                SourceId = index + 1,
                Title = hit.Source,
                Url = hit.Source,
                Excerpt = hit.Text.Length > 200 ? hit.Text[..200] + "..." : hit.Text,
                Score = hit.Score
            }).ToList();

            // Save messages to database using background queue
            await _backgroundTaskQueue.QueueBackgroundWorkItemAsync(async token =>
            {
                try
                {
                    // Create a new scope to resolve scoped services (DbContext)
                    using var scope = _serviceScopeFactory.CreateScope();
                    var messageRepo = scope.ServiceProvider.GetRequiredService<IMessageRepository>();
                    var conversationRepo = scope.ServiceProvider.GetRequiredService<IConversationRepository>();

                    // Save user message
                    _ = await messageRepo.CreateAsync(new Message
                    {
                        ConversationId = dbConversationId,
                        Role = "user",
                        Content = userQuestion,
                        CreatedAt = DateTime.UtcNow
                    });

                    // Save assistant message with sources
                    var sourcesList = sources.Select(s => new MessageSource
                    {
                        SourceId = s.SourceId,
                        Title = s.Title,
                        Url = s.Url,
                        Excerpt = s.Excerpt,
                        Score = s.Score
                    }).ToList();

                    _ = await messageRepo.CreateWithSourcesAsync(new Message
                    {
                        ConversationId = dbConversationId,
                        Role = "assistant",
                        Content = assistantAnswer,
                        CreatedAt = DateTime.UtcNow
                    }, sourcesList);

                    // Update conversation timestamp
                    var conversation = await conversationRepo.GetByConversationIdAsync(conversationGuid, userId);
                    if (conversation != null)
                    {
                        conversation.UpdatedAt = DateTime.UtcNow;
                        await conversationRepo.UpdateAsync(conversation);
                    }

                    _logger.LogInformation(
                        "Successfully saved conversation messages for conversation ID: {ConversationId}",
                        conversationGuid
                        );
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Failed to save conversation messages to database for conversation ID: {ConversationId}",
                        conversationGuid
                        );
                }
            });
        }
    }
}
