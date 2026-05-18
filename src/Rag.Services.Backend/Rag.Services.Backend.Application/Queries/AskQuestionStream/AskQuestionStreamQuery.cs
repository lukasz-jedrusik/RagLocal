using MediatR;

namespace Rag.Services.Backend.Application.Queries.AskQuestionStream
{
    public class AskQuestionStreamQuery : IStreamRequest<string>
    {
        public string Question { get; set; } = string.Empty;
        public string ConversationId { get; set; } = string.Empty;
    }
}
