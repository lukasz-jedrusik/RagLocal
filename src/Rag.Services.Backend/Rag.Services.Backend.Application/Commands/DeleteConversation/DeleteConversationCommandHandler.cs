using MediatR;
using Rag.Services.Backend.Application.Interfaces.Repositories;
using Rag.Services.Backend.Domain.Exceptions;

namespace Rag.Services.Backend.Application.Commands.DeleteConversation
{
    public class DeleteConversationCommandHandler(
        IConversationRepository conversationRepository) : IRequestHandler<DeleteConversationCommand, bool>
    {
        private readonly IConversationRepository _conversationRepository = conversationRepository;

        public async Task<bool> Handle(DeleteConversationCommand request, CancellationToken cancellationToken)
        {
            var conversation = await _conversationRepository.GetByConversationIdAsync(request.ConversationId, request.UserId);

            if (conversation == null)
            {
                throw new BackendException("Conversation not found or access denied");
            }

            await _conversationRepository.DeleteAsync(conversation.Id);
            return true;
        }
    }
}
