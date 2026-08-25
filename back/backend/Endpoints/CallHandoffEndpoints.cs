using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using backend.Dtos;
using backend.Services;

namespace backend.Endpoints
{
    public static class CallHandoffEndpoints
    {
        public static void MapCallHandoffEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/calls/{callSessionId:guid}/handoffs");

            group.MapGet("/", async (
                Guid callSessionId,
                CallHandoffService service) =>
            {
                var result = await service.ListForCallAsync(callSessionId);
                return Results.Ok(result);
            });

            group.MapGet("/{handoffId:guid}", async (
                Guid callSessionId,
                Guid handoffId,
                CallHandoffService service) =>
            {
                var result = await service.GetByIdAsync(handoffId);
                return result is not null ? Results.Ok(result) : Results.NotFound();
            });

            group.MapPost("/{transferId:guid}", async (
                Guid callSessionId,
                Guid transferId,
                [FromBody] CreateHandoffRequest request,
                CallHandoffService service) =>
            {
                var result = await service.CreateContextAsync(
                    transferId,
                    request.Summary,
                    request.ContextDataJson,
                    request.Reason);
                return result is not null ? Results.Ok(result) : Results.NotFound();
            });

            group.MapPost("/{handoffId:guid}/deliver", async (
                Guid callSessionId,
                Guid handoffId,
                CallHandoffService service) =>
            {
                var result = await service.DeliverAsync(handoffId);
                return result is not null ? Results.Ok(result) : Results.NotFound();
            });

            group.MapPost("/{handoffId:guid}/accept", async (
                Guid callSessionId,
                Guid handoffId,
                CallHandoffService service) =>
            {
                var result = await service.AcceptAsync(handoffId);
                return result is not null ? Results.Ok(result) : Results.NotFound();
            });
        }
    }
}