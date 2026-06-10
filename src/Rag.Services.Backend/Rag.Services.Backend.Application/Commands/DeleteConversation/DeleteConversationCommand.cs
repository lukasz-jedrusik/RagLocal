using MediatR;

namespace Rag.Services.Backend.Application.Commands.DeleteConversation
{
    public class DeleteConversationCommand : IRequest<bool>
    {
        public string ConversationId { get; set; } = string.Empty;
        public int UserId { get; set; }
    }
}
