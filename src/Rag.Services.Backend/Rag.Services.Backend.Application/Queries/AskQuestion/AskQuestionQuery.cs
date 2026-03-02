using MediatR;

namespace Rag.Services.Backend.Application.Queries.AskQuestion
{
    public class AskQuestionQuery : IRequest<string>
    {
        public string Question { get; set; }
    }
}