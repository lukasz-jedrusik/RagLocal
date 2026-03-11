using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Rag.Services.Backend.Application.Interfaces.Services;
using Rag.Services.Backend.Application.Services;

namespace Rag.Services.Backend.Application.Commands.IngestFiles
{
    public class IngestFilesCommandHandler(
        ILogger<IngestFilesCommandHandler> logger,
        IBackgroundTaskQueue backgroundTaskQueue,
        IServiceScopeFactory serviceScopeFactory)
        : IRequestHandler<IngestFilesCommand>
    {
        private readonly ILogger<IngestFilesCommandHandler> _logger = logger;
        private readonly IBackgroundTaskQueue _backgroundTaskQueue = backgroundTaskQueue;
        private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;

        public async Task Handle(IngestFilesCommand request, CancellationToken cancellationToken)
        {
            await _backgroundTaskQueue.QueueBackgroundWorkItemAsync(async token =>
            {
                // Create a new scope to resolve scoped services
                using var scope = _serviceScopeFactory.CreateScope();

                // Resolve the required services within the scope
                var qdrantStore = scope.ServiceProvider.GetRequiredService<IQdrantService>();
                var ollamaService = scope.ServiceProvider.GetRequiredService<IOllamaService>();
                var pdfLoaderService = scope.ServiceProvider.GetRequiredService<IPdfLoaderService>();
                var wordLoaderService = scope.ServiceProvider.GetRequiredService<IWordLoaderService>();

                // Log the start of the file ingestion process
                _logger.LogInformation("file ingestion process started");

                //  Initialize Qdrant collection
                await qdrantStore.InitAsync();

                // Ingest files from the docs directory
                var docsPath = "../../../docs";

                // Ensure the docs directory exists
                Directory.CreateDirectory(docsPath);

                // Process each file in the docs directory
                foreach (var file in Directory.GetFiles(docsPath))
                {
                    _logger.LogInformation("Reading {File}", file);

                    // Load the file content based on its type and create vector embeddings
                    string text = file switch
                    {
                        var f when f.EndsWith(".pdf") => pdfLoaderService.Load(f),
                        var f when f.EndsWith(".docx") => wordLoaderService.Load(f),
                        var f when f.EndsWith(".txt") => await File.ReadAllTextAsync(f, token),
                        _ => ""
                    };

                    // Chunk the text and create vector embeddings for each chunk, then store them in Qdrant
                    var chunks = TextChunker.Chunk(text);

                    // Add each chunk to Qdrant with its corresponding vector embedding
                    foreach (var chunk in chunks)
                    {
                        var vector = await ollamaService.CreateAsync(chunk);
                        await qdrantStore.AddAsync(chunk, Path.GetFileName(file), vector);
                    }
                }

                // Log the completion of the file ingestion process
                _logger.LogInformation("file ingestion process completed");
            });
        }
    }
}