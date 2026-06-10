using MediatR;
using Rag.Services.Backend.Application.DataTransferObjects;
using Rag.Services.Backend.Application.Interfaces.Repositories;
using Rag.Services.Backend.Application.Interfaces.Services;
using Rag.Services.Backend.Domain.Models;

namespace Rag.Services.Backend.Application.Queries.AskQuestion
{
    public class AskQuestionQueryHandler(
        IQdrantService vectorStore,
        IOllamaService ollama,
        IConversationService conversationService,
        IConversationRepository conversationRepository,
        IMessageRepository messageRepository) : IRequestHandler<AskQuestionQuery, AskResponseDto>
    {
        private readonly IQdrantService _vectorStore = vectorStore;
        private readonly IOllamaService _ollamaService = ollama;
        private readonly IConversationService _conversationService = conversationService;
        private readonly IConversationRepository _conversationRepository = conversationRepository;
        private readonly IMessageRepository _messageRepository = messageRepository;

        public async Task<AskResponseDto> Handle(
            AskQuestionQuery request,
            CancellationToken cancellationToken)
        {
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
                    List<Message> dbMessages = await _messageRepository.GetConversationMessagesAsync(dbConversation.Id);
                    history = dbMessages.ConvertAll(static m => new ConversationMessage
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
                        foreach (ConversationMessage msg in history)
                        {
                            await _conversationService.AddMessageAsync(conversationId, msg.Role, msg.Content);
                        }
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
            float[] qVec = await _ollamaService.CreateAsync(request.Question);
            List<SearchResult> hits = await _vectorStore.SearchAsync(qVec);
            string context = string.Join("\n\n", hits.Select(static h => h.Text));

            // Get answer with conversation history
            string answer = await _ollamaService.AskWithHistoryAsync(context, history, request.Question);

            // Add user question and assistant answer to conversation history (in-memory)
            await _conversationService.AddMessageAsync(conversationId, "user", request.Question);
            await _conversationService.AddMessageAsync(conversationId, "assistant", answer);

            // Save messages to database
            _ = await _messageRepository.CreateAsync(new Message
            {
                ConversationId = dbConversation.Id,
                Role = "user",
                Content = request.Question,
                CreatedAt = DateTime.UtcNow
            });

            var sourcesList = hits.Select((hit, index) => new MessageSource
            {
                SourceId = index + 1,
                Title = hit.Source,
                Url = hit.Source,
                Excerpt = hit.Text.Length > 200 ? hit.Text[..200] + "..." : hit.Text,
                Score = hit.Score
            }).ToList();

            _ = await _messageRepository.CreateWithSourcesAsync(new Message
            {
                ConversationId = dbConversation.Id,
                Role = "assistant",
                Content = answer,
                CreatedAt = DateTime.UtcNow
            }, sourcesList);

            // Update conversation timestamp
            dbConversation.UpdatedAt = DateTime.UtcNow;
            await _conversationRepository.UpdateAsync(dbConversation);

            return new AskResponseDto
            {
                Answer = answer,
                ConversationId = conversationId,
                Sources = hits.Select((hit, index) => new SourceDto
                {
                    Id = index + 1,
                    Title = hit.Source,
                    Url = hit.Source,
                    Excerpt = hit.Text.Length > 200 ? hit.Text[..200] + "..." : hit.Text,
                    Score = hit.Score
                }).ToList()
            };
        }
    }
}