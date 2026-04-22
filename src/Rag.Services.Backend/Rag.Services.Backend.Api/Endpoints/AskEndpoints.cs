using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Rag.Services.Backend.Application.DataTransferObjects;
using Rag.Services.Backend.Application.Queries.AskQuestion;

namespace Rag.Services.Backend.Api.Endpoints
{
    public static class AskEndpoints
    {
        public static void AddAskEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapPost(
                "/ask",
                async ([Required][FromBody] AskRequestDto askRequestDto, IMediator mediator) =>
                {
                    var query = new AskQuestionQuery
                    {
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
                .Produces(StatusCodes.Status400BadRequest);
        }
    }
}