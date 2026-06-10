using System.ComponentModel.DataAnnotations;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Rag.Services.Backend.Application.Commands.CreateConversation;
using Rag.Services.Backend.Application.Commands.DeleteConversation;
using Rag.Services.Backend.Application.DataTransferObjects;
using Rag.Services.Backend.Application.Interfaces.Services;
using Rag.Services.Backend.Application.Queries.GetConversationDetail;
using Rag.Services.Backend.Application.Queries.GetConversations;

namespace Rag.Services.Backend.Api.Endpoints
{
    public static class ConversationEndpoints
    {
        public static void AddConversationEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet(
                "/conversations",
                async (
                    HttpContext context,
                    IIdentityService identityService,
                    IMediator mediator) =>
                {
                    var userId = identityService.GetUserId(context);

                    var query = new GetConversationsQuery
                    {
                        UserId = userId
                    };

                    var result = await mediator.Send(query);
                    return Results.Ok(result);
                })
                .WithName("GetConversations")
                .WithTags("Conversations")
                .WithSummary("Get all user conversations")
                .WithDescription("Retrieve all conversations for the authenticated user")
                .Produces<List<ConversationDto>>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status401Unauthorized)
                .RequireAuthorization();

            app.MapGet(
                "/conversations/{conversationId}",
                async (
                    string conversationId,
                    HttpContext context,
                    IIdentityService identityService,
                    IMediator mediator) =>
                {
                    var userId = identityService.GetUserId(context);

                    var query = new GetConversationDetailQuery
                    {
                        ConversationId = conversationId,
                        UserId = userId
                    };

                    var result = await mediator.Send(query);
                    return Results.Ok(result);
                })
                .WithName("GetConversationDetail")
                .WithTags("Conversations")
                .WithSummary("Get conversation details")
                .WithDescription("Retrieve full conversation with all messages")
                .Produces<ConversationDetailDto>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status404NotFound)
                .RequireAuthorization();

            app.MapPost(
                "/conversations",
                async (
                    [Required][FromBody] CreateConversationDto request,
                    HttpContext context,
                    IIdentityService identityService,
                    IMediator mediator) =>
                {
                    var userId = identityService.GetUserId(context);

                    var command = new CreateConversationCommand
                    {
                        UserId = userId,
                        Title = request.Title
                    };

                    var result = await mediator.Send(command);
                    return Results.Created($"/conversations/{result.ConversationId}", result);
                })
                .WithName("CreateConversation")
                .WithTags("Conversations")
                .WithSummary("Create new conversation")
                .WithDescription("Create a new conversation for the authenticated user")
                .Produces<ConversationDto>(StatusCodes.Status201Created)
                .Produces(StatusCodes.Status401Unauthorized)
                .RequireAuthorization();

            app.MapDelete(
                "/conversations/{conversationId}",
                async (
                    string conversationId,
                    HttpContext context,
                    IIdentityService identityService,
                    IMediator mediator) =>
                {
                    var userId = identityService.GetUserId(context);

                    var command = new DeleteConversationCommand
                    {
                        ConversationId = conversationId,
                        UserId = userId
                    };

                    await mediator.Send(command);
                    return Results.NoContent();
                })
                .WithName("DeleteConversation")
                .WithTags("Conversations")
                .WithSummary("Delete conversation")
                .WithDescription("Delete (soft delete) a conversation")
                .Produces(StatusCodes.Status204NoContent)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status404NotFound)
                .RequireAuthorization();
        }
    }
}
