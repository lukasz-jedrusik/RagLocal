using MediatR;
using Rag.Services.Backend.Application.DataTransferObjects;

namespace Rag.Services.Backend.Application.Queries.GetConversations
{
    public class GetConversationsQuery : IRequest<List<ConversationDto>>
    {
        public int UserId { get; set; }
    }
}
