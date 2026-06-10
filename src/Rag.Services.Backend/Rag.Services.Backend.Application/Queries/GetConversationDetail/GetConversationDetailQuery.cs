using MediatR;
using Rag.Services.Backend.Application.DataTransferObjects;

namespace Rag.Services.Backend.Application.Queries.GetConversationDetail
{
    public class GetConversationDetailQuery : IRequest<ConversationDetailDto>
    {
        public string ConversationId { get; set; } = string.Empty;
        public int UserId { get; set; }
    }
}
