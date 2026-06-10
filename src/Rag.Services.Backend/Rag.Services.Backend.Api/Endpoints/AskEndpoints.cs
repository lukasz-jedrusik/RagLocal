using System.ComponentModel.DataAnnotations;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Rag.Services.Backend.Application.DataTransferObjects;
using Rag.Services.Backend.Application.Interfaces.Services;
using Rag.Services.Backend.Application.Queries.AskQuestion;
using Rag.Services.Backend.Application.Queries.AskQuestionStream;

namespace Rag.Services.Backend.Api.Endpoints
{
    public static class AskEndpoints
    {
        public static void AddAskEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapPost(
                "/ask",
                async (
                    [Required][FromBody] AskRequestDto askRequestDto,
                    HttpContext context,
                    IIdentityService identityService,
                    IMediator mediator) =>
                {
                    var userId = identityService.GetUserId(context);

                    var query = new AskQuestionQuery
                    {
                        UserId = userId,
                        Question = askRequestDto.Question,
                        ConversationId = askRequestDto.ConversationId
                    };

                    var result = await mediator.Send(query);
                    return Results.Ok(result);
                })
                .WithName("AskQuestion")
                .WithTags("Questions")
                .WithSummary("Ask a question")
                .WithDescription("Submit a question and get an AI-generated answer with conversation context")
                .Produces<AskResponseDto>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized)
                .RequireAuthorization();

            app.MapPost(
                "/ask/stream",
                async (
                    [Required][FromBody] AskRequestDto askRequestDto,
                    HttpContext context,
                    IIdentityService identityService,
                    IMediator mediator,
                    CancellationToken cancellationToken) =>
                {
                    var userId = identityService.GetUserId(context);

                    var query = new AskQuestionStreamQuery
                    {
                        UserId = userId,
                        Question = askRequestDto.Question,
                        ConversationId = askRequestDto.ConversationId,
                        Response = context.Response,
                        CancellationToken = cancellationToken
                    };

                    await mediator.Send(query, cancellationToken);
                })
                .WithName("AskQuestionStream")
                .WithTags("Questions")
                .WithSummary("Ask a question with streaming response")
                .WithDescription("Submit a question and get an AI-generated answer streamed as Server-Sent Events with metadata, tokens, and citations")
                .Produces(StatusCodes.Status200OK, contentType: "text/event-stream")
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized)
                .RequireAuthorization();
        }
    }
}