using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models.Enums;
using backend.Services;

namespace backend.Endpoints
{
    public static class LiveKitEndpoints
    {
        public static void MapLiveKitEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/livekit");

            group.MapPost("/token", async (
                [FromBody] LiveKitTokenRequest request,
                LiveKitService service,
                AppDbContext db) =>
            {
                if (request.Identity.StartsWith("agent_"))
                {
                    var agentIdStr = request.Identity["agent_".Length..];
                    if (!Guid.TryParse(agentIdStr, out var agentId))
                        return Results.BadRequest(new { error = "Invalid agent identity" });

                    var hasTransfer = await db.CallTransfers.AnyAsync(t =>
                        t.Status == CallTransferStatus.Accepted &&
                        t.ToHumanAgentId == agentId &&
                        t.CallSession.LivekitRoomName == request.RoomName);

                    if (!hasTransfer)
                        return Results.Forbid();
                }

                var token = service.GenerateToken(
                    request.Identity,
                    request.RoomName,
                    request.CanPublish,
                    request.CanSubscribe);
                return Results.Ok(new { token });
            });

            group.MapPost("/room", async (
                [FromBody] LiveKitRoomRequest request,
                LiveKitService service) =>
            {
                var result = await service.CreateRoom(request.RoomName);
                return Results.Ok(new { room = result });
            });

            group.MapDelete("/room/{roomName}", async (
                string roomName,
                LiveKitService service) =>
            {
                var result = await service.DeleteRoom(roomName);
                return Results.Ok(new { room = result });
            });

            group.MapPost("/room/{roomName}/egress/start", async (
                string roomName,
                LiveKitService service) =>
            {
                var result = await service.StartEgress(roomName);
                return Results.Ok(new { egress = result });
            });

            group.MapPost("/room/{roomName}/egress/stop", async (
                string roomName,
                LiveKitService service) =>
            {
                var result = await service.StopEgress(roomName);
                return Results.Ok(new { egress = result });
            });
        }
    }

    public sealed record LiveKitTokenRequest(
        string Identity,
        string RoomName,
        bool CanPublish = true,
        bool CanSubscribe = true
    );

    public sealed record LiveKitRoomRequest(string RoomName);
}