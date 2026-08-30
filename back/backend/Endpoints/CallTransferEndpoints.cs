using Microsoft.AspNetCore.Mvc;
using MediatR;
using backend.Dtos;
using backend.Modules.CallOperations.Features.CallTransfers.ListCallTransfers;
using backend.Modules.CallOperations.Features.CallTransfers.GetCallTransfer;
using backend.Modules.CallOperations.Features.CallTransfers.InitiateTransfer;
using backend.Modules.CallOperations.Features.CallTransfers.AcceptTransfer;
using backend.Modules.CallOperations.Features.CallTransfers.RejectTransfer;
using backend.Modules.CallOperations.Features.CallTransfers.CompleteTransfer;

namespace backend.Endpoints
{
    public static class CallTransferEndpoints
    {
        public static void MapCallTransferEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/calls/{callSessionId:guid}/transfers");

            group.MapGet("/", async (
                Guid callSessionId,
                IMediator mediator) =>
            {
                var result = await mediator.Send(new ListCallTransfersQuery(callSessionId));
                return Results.Ok(result);
            });

            group.MapGet("/{transferId:guid}", async (
                Guid callSessionId,
                Guid transferId,
                IMediator mediator) =>
            {
                var result = await mediator.Send(new GetCallTransferQuery(callSessionId, transferId));
                return result is not null ? Results.Ok(result) : Results.NotFound();
            });

            group.MapPost("/", async (
                Guid callSessionId,
                [FromBody] InitiateTransferRequest request,
                IMediator mediator,
                HttpContext http) =>
            {
                var userId = (Guid)http.Items["UserId"]!;
                try
                {
                    var result = await mediator.Send(new InitiateTransferCommand(
                        callSessionId, userId, request.TargetType, request.TargetName, request.Reason));

                    if (result is null) return Results.NotFound();

                    if (result is CallTransferDto destResult)
                    {
                        return Results.Created(
                            $"/api/calls/{callSessionId}/transfers/{destResult.Id}",
                            new { transfer = destResult });
                    }
                    else if (result is TransferResponse tr)
                    {
                        return Results.Created($"/api/calls/{callSessionId}/transfers/{tr.Transfer.Id}", tr);
                    }

                    return Results.NotFound();
                }
                catch (InvalidOperationException ex)
                {
                    return Results.BadRequest(ex.Message);
                }
            });

            group.MapPost("/{transferId:guid}/accept", async (
                Guid callSessionId,
                Guid transferId,
                [FromBody] AcceptTransferBody body,
                IMediator mediator) =>
            {
                var result = await mediator.Send(new AcceptTransferCommand(transferId, body.HumanAgentId));
                return result is not null ? Results.Ok(result) : Results.NotFound();
            });

            group.MapPost("/{transferId:guid}/reject", async (
                Guid callSessionId,
                Guid transferId,
                [FromBody] RejectTransferBody body,
                IMediator mediator) =>
            {
                var result = await mediator.Send(new RejectTransferCommand(transferId, body.HumanAgentId));
                return result is not null ? Results.Ok(result) : Results.NotFound();
            });

            group.MapPost("/{transferId:guid}/complete", async (
                Guid callSessionId,
                Guid transferId,
                IMediator mediator) =>
            {
                var result = await mediator.Send(new CompleteTransferCommand(transferId));
                return result is not null ? Results.Ok(result) : Results.NotFound();
            });
        }
    }

    public sealed record AcceptTransferBody(Guid HumanAgentId);
    public sealed record RejectTransferBody(Guid HumanAgentId);
}