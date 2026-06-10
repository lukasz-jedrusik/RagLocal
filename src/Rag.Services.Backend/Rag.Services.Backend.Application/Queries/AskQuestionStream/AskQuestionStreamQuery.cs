using MediatR;
using Microsoft.AspNetCore.Http;

namespace Rag.Services.Backend.Application.Queries.AskQuestionStream
{
    public class AskQuestionStreamQuery : IRequest
    {
        public int UserId { get; set; }
        public string Question { get; set; } = string.Empty;
        public string ConversationId { get; set; } = string.Empty;
        public required HttpResponse Response { get; set; }
        public CancellationToken CancellationToken { get; set; }
    }
}
