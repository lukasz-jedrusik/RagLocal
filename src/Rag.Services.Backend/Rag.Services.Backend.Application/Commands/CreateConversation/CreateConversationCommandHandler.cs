using MediatR;
using Rag.Services.Backend.Application.DataTransferObjects;
using Rag.Services.Backend.Application.Interfaces.Repositories;
using Rag.Services.Backend.Domain.Models;

namespace Rag.Services.Backend.Application.Commands.CreateConversation
{
    public class CreateConversationCommandHandler(
        IConversationRepository conversationRepository) : IRequestHandler<CreateConversationCommand, ConversationDto>
    {
        private readonly IConversationRepository _conversationRepository = conversationRepository;

        public async Task<ConversationDto> Handle(CreateConversationCommand request, CancellationToken cancellationToken)
        {
            var conversation = new Conversation
            {
                ConversationId = Guid.NewGuid().ToString(),
                UserId = request.UserId,
                Title = string.IsNullOrWhiteSpace(request.Title) ? "New Conversation" : request.Title,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var created = await _conversationRepository.CreateAsync(conversation);

            return new ConversationDto
            {
                ConversationId = created.ConversationId,
                Title = created.Title,
                CreatedAt = created.CreatedAt,
                UpdatedAt = created.UpdatedAt,
                MessageCount = 0
            };
        }
    }
}
