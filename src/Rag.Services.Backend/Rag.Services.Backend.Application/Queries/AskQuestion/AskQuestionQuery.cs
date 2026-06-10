using MediatR;
using Rag.Services.Backend.Application.DataTransferObjects;

namespace Rag.Services.Backend.Application.Queries.AskQuestion
{
    public class AskQuestionQuery : IRequest<AskResponseDto>
    {
        public int UserId { get; set; }
        public string Question { get; set; } = string.Empty;
        public string ConversationId { get; set; } = string.Empty;
    }
}