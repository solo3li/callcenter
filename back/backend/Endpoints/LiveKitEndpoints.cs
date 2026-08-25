using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
                LiveKitService service) =>
            {
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