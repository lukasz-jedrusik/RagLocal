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
                using var scope = _serviceScopeFactory.CreateScope();
                var qdrantStore = scope.ServiceProvider.GetRequiredService<IQdrantStore>();
                var ollamaService = scope.ServiceProvider.GetRequiredService<IOllamaService>();

                _logger.LogInformation("file ingestion process started");

                await qdrantStore.InitAsync();

                var docsPath = "../../../docs";
                Directory.CreateDirectory(docsPath);

                foreach (var file in Directory.GetFiles(docsPath))
                {
                    _logger.LogInformation("Reading {File}", file);

                    var text = await File.ReadAllTextAsync(file, token);

                    var chunks = TextChunker.Chunk(text);

                    foreach (var chunk in chunks)
                    {
                        var vector = await ollamaService.CreateAsync(chunk);
                        await qdrantStore.AddAsync(chunk, Path.GetFileName(file), vector);
                    }
                }

                _logger.LogInformation("file ingestion process completed");
            });
        }
    }
}