using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using backend.Dtos;
using backend.Services;

namespace backend.Endpoints
{
    public static class CallTransferEndpoints
    {
        public static void MapCallTransferEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/calls/{callSessionId:guid}/transfers");

            group.MapGet("/", async (
                Guid callSessionId,
                CallTransferService service) =>
            {
                var result = await service.ListForCallAsync(callSessionId);
                return Results.Ok(result);
            });

            group.MapGet("/{transferId:guid}", async (
                Guid callSessionId,
                Guid transferId,
                CallTransferService service) =>
            {
                var result = await service.GetByIdAsync(callSessionId, transferId);
                return result is not null ? Results.Ok(result) : Results.NotFound();
            });

            group.MapPost("/", async (
                Guid callSessionId,
                [FromBody] InitiateTransferRequest request,
                CallTransferService service,
                HttpContext http) =>
            {
                var userId = (Guid)http.Items["UserId"]!;
                try
                {
                    var targetType = request.TargetType?.Trim().ToLowerInvariant();

                    if (targetType == "destination")
                    {
                        if (string.IsNullOrWhiteSpace(request.TargetName))
                            return Results.BadRequest("TargetName is required for destination transfers");

                        var destResult = await service.InitiateDestinationTransferAsync(
                            callSessionId, userId, request.TargetName!, request.Reason);
                        return destResult is not null
                            ? Results.Created(
                                $"/api/calls/{callSessionId}/transfers/{destResult.Id}",
                                new { transfer = destResult })
                            : Results.NotFound();
                    }

                    var result = await service.InitiateTransferAsync(callSessionId, userId, request.Reason);
                    return result is not null
                        ? Results.Created($"/api/calls/{callSessionId}/transfers/{result.Transfer.Id}", result)
                        : Results.NotFound();
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
                CallTransferService service) =>
            {
                var result = await service.AcceptTransferAsync(transferId, body.HumanAgentId);
                return result is not null ? Results.Ok(result) : Results.NotFound();
            });

            group.MapPost("/{transferId:guid}/reject", async (
                Guid callSessionId,
                Guid transferId,
                [FromBody] RejectTransferBody body,
                CallTransferService service) =>
            {
                var result = await service.RejectTransferAsync(transferId, body.HumanAgentId);
                return result is not null ? Results.Ok(result) : Results.NotFound();
            });

            group.MapPost("/{transferId:guid}/complete", async (
                Guid callSessionId,
                Guid transferId,
                CallTransferService service) =>
            {
                var result = await service.CompleteTransferAsync(transferId);
                return result is not null ? Results.Ok(result) : Results.NotFound();
            });
        }
    }

    public sealed record AcceptTransferBody(Guid HumanAgentId);
    public sealed record RejectTransferBody(Guid HumanAgentId);
}