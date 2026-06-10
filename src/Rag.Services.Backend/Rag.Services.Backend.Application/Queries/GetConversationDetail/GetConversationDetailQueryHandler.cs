using MediatR;
using Rag.Services.Backend.Application.DataTransferObjects;
using Rag.Services.Backend.Application.Interfaces.Repositories;
using Rag.Services.Backend.Domain.Exceptions;

namespace Rag.Services.Backend.Application.Queries.GetConversationDetail
{
    public class GetConversationDetailQueryHandler(
        IConversationRepository conversationRepository,
        IMessageRepository messageRepository) : IRequestHandler<GetConversationDetailQuery, ConversationDetailDto>
    {
        private readonly IConversationRepository _conversationRepository = conversationRepository;
        private readonly IMessageRepository _messageRepository = messageRepository;

        public async Task<ConversationDetailDto> Handle(GetConversationDetailQuery request, CancellationToken cancellationToken)
        {
            var conversation = await _conversationRepository.GetByConversationIdAsync(request.ConversationId, request.UserId);

            if (conversation == null)
            {
                throw new BackendException("Conversation not found or access denied");
            }

            var messages = await _messageRepository.GetConversationMessagesAsync(conversation.Id);

            return new ConversationDetailDto
            {
                ConversationId = conversation.ConversationId,
                Title = conversation.Title,
                CreatedAt = conversation.CreatedAt,
                UpdatedAt = conversation.UpdatedAt,
                Messages = messages.Select(
                    m => new MessageDto
                    {
                        Role = m.Role,
                        Content = m.Content,
                        CreatedAt = m.CreatedAt,
                        Sources = m.Sources.Select(s => new SourceDto
                        {
                            Id = s.SourceId,
                            Title = s.Title,
                            Url = s.Url,
                            Excerpt = s.Excerpt,
                            Score = s.Score
                        }).ToList()
                    }).ToList()
            };
        }
    }
}
