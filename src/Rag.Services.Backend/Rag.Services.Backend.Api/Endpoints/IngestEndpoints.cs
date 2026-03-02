using MediatR;
using Rag.Services.Backend.Application.Commands.IngestFiles;

namespace Rag.Services.Backend.Api.Endpoints
{
    public static class IngestEndpoints
    {
        public static void AddIngestEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapPost(
                "/ingest",
                async (IMediator mediator) =>
                {
                    var command = new IngestFilesCommand();
                    await mediator.Send(command);
                    return Results.Accepted();
                })
                .WithName("IngestFiles")
                .WithTags("Data Ingestion")
                .WithSummary("Ingest files")
                .WithDescription("Process and ingest files for indexing")
                .Produces(StatusCodes.Status202Accepted)
                .Produces(StatusCodes.Status400BadRequest);
        }
    }
}