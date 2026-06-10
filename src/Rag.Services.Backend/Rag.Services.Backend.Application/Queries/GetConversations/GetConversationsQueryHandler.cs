using MediatR;
using Rag.Services.Backend.Application.DataTransferObjects;
using Rag.Services.Backend.Application.Interfaces.Repositories;

namespace Rag.Services.Backend.Application.Queries.GetConversations
{
    public class GetConversationsQueryHandler(
        IConversationRepository conversationRepository,
        IMessageRepository messageRepository) : IRequestHandler<GetConversationsQuery, List<ConversationDto>>
    {
        private readonly IConversationRepository _conversationRepository = conversationRepository;
        private readonly IMessageRepository _messageRepository = messageRepository;

        public async Task<List<ConversationDto>> Handle(GetConversationsQuery request, CancellationToken cancellationToken)
        {
            var conversations = await _conversationRepository.GetUserConversationsAsync(request.UserId);

            var conversationDtos = new List<ConversationDto>();

            foreach (var conversation in conversations)
            {
                var messageCount = await _messageRepository.GetMessageCountAsync(conversation.Id);

                conversationDtos.Add(new ConversationDto
                {
                    ConversationId = conversation.ConversationId,
                    Title = conversation.Title,
                    CreatedAt = conversation.CreatedAt,
                    UpdatedAt = conversation.UpdatedAt,
                    MessageCount = messageCount
                });
            }

            return conversationDtos;
        }
    }
}
