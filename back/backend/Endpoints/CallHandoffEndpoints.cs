using Microsoft.AspNetCore.Mvc;
using MediatR;
using backend.Dtos;
using backend.Modules.CallOperations.Features.CallHandoffs.ListCallHandoffs;
using backend.Modules.CallOperations.Features.CallHandoffs.GetCallHandoff;
using backend.Modules.CallOperations.Features.CallHandoffs.CreateCallHandoff;
using backend.Modules.CallOperations.Features.CallHandoffs.DeliverCallHandoff;
using backend.Modules.CallOperations.Features.CallHandoffs.AcceptCallHandoff;

namespace backend.Endpoints
{
    public static class CallHandoffEndpoints
    {
        public static void MapCallHandoffEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/calls/{callSessionId:guid}/handoffs");

            group.MapGet("/", async (
                Guid callSessionId,
                IMediator mediator) =>
            {
                var result = await mediator.Send(new ListCallHandoffsQuery(callSessionId));
                return Results.Ok(result);
            });

            group.MapGet("/{handoffId:guid}", async (
                Guid callSessionId,
                Guid handoffId,
                IMediator mediator) =>
            {
                var result = await mediator.Send(new GetCallHandoffQuery(handoffId));
                return result is not null ? Results.Ok(result) : Results.NotFound();
            });

            group.MapPost("/{transferId:guid}", async (
                Guid callSessionId,
                Guid transferId,
                [FromBody] CreateHandoffRequest request,
                IMediator mediator) =>
            {
                var result = await mediator.Send(new CreateCallHandoffCommand(
                    transferId,
                    request.Summary,
                    request.ContextDataJson,
                    request.Reason));
                return result is not null ? Results.Ok(result) : Results.NotFound();
            });

            group.MapPost("/{handoffId:guid}/deliver", async (
                Guid callSessionId,
                Guid handoffId,
                IMediator mediator) =>
            {
                var result = await mediator.Send(new DeliverCallHandoffCommand(handoffId));
                return result is not null ? Results.Ok(result) : Results.NotFound();
            });

            group.MapPost("/{handoffId:guid}/accept", async (
                Guid callSessionId,
                Guid handoffId,
                IMediator mediator) =>
            {
                var result = await mediator.Send(new AcceptCallHandoffCommand(handoffId));
                return result is not null ? Results.Ok(result) : Results.NotFound();
            });
        }
    }
}