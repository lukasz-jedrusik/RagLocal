using MediatR;
using Rag.Services.Backend.Application.Interfaces.Services;

namespace Rag.Services.Backend.Application.Queries.AskQuestion
{
    public class AskQuestionQueryHandler(
        IQdrantStore vectorStore,
        IOllamaService ollama) : IRequestHandler<AskQuestionQuery, string>
    {
        private readonly IQdrantStore _vectorStore = vectorStore;
        private readonly IOllamaService _ollamaService = ollama;

        public async Task<string> Handle(
            AskQuestionQuery request,
            CancellationToken cancellationToken)
        {
            var qVec = await _ollamaService.CreateAsync(request.Question);

            var hits = await _vectorStore.SearchAsync(qVec);

            var context = string.Join(
                "\n\n",
                hits.Select(h => h.Text));

            var prompt = $"""
            Answer ONLY using context below:

            {context}

            Question: {request.Question}
            """;

            return await _ollamaService.AskAsync(prompt);
        }
    }
}