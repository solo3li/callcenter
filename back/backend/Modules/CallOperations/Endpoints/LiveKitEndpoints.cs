using Microsoft.AspNetCore.Mvc;
using MediatR;
using backend.Modules.CallOperations.Features.LiveKit.GenerateLiveKitToken;
using backend.Modules.CallOperations.Features.LiveKit.CreateLiveKitRoom;
using backend.Modules.CallOperations.Features.LiveKit.DeleteLiveKitRoom;
using backend.Modules.CallOperations.Features.LiveKit.StartLiveKitEgress;
using backend.Modules.CallOperations.Features.LiveKit.StopLiveKitEgress;

namespace backend.Modules.CallOperations.Endpoints
{
    public static class LiveKitEndpoints
    {
        public static void MapLiveKitEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/livekit");

            group.MapPost("/token", async (
                [FromBody] LiveKitTokenRequest request,
                IMediator mediator) =>
            {
                try
                {
                    var token = await mediator.Send(new GenerateLiveKitTokenCommand(
                        request.Identity,
                        request.RoomName,
                        request.CanPublish,
                        request.CanSubscribe));
                    return Results.Ok(new { token });
                }
                catch (UnauthorizedAccessException)
                {
                    return Results.Forbid();
                }
                catch (InvalidOperationException ex)
                {
                    return Results.BadRequest(new { error = ex.Message });
                }
            });

            group.MapPost("/room", async (
                [FromBody] LiveKitRoomRequest request,
                IMediator mediator) =>
            {
                var result = await mediator.Send(new CreateLiveKitRoomCommand(request.RoomName));
                return Results.Ok(new { room = result });
            });

            group.MapDelete("/room/{roomName}", async (
                string roomName,
                IMediator mediator) =>
            {
                var result = await mediator.Send(new DeleteLiveKitRoomCommand(roomName));
                return Results.Ok(new { room = result });
            });

            group.MapPost("/room/{roomName}/egress/start", async (
                string roomName,
                IMediator mediator) =>
            {
                var result = await mediator.Send(new StartLiveKitEgressCommand(roomName));
                return Results.Ok(new { egress = result });
            });

            group.MapPost("/room/{roomName}/egress/stop", async (
                string roomName,
                IMediator mediator) =>
            {
                var result = await mediator.Send(new StopLiveKitEgressCommand(roomName));
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