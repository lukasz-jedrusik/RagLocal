using MediatR;
using Rag.Services.Backend.Application.DataTransferObjects;

namespace Rag.Services.Backend.Application.Commands.CreateConversation
{
    public class CreateConversationCommand : IRequest<ConversationDto>
    {
        public int UserId { get; set; }
        public string Title { get; set; } = string.Empty;
    }
}
