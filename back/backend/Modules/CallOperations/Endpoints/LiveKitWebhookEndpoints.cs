using System.Text.Json;
using MediatR;
using backend.Middleware;
using backend.Services;
using backend.Modules.CallOperations.Features.CallRouting.HandleParticipantJoined;
using backend.Modules.CallOperations.Features.CallRouting.HandleParticipantLeft;
using backend.Modules.CallOperations.Features.CallRouting.HandleRoomFinished;

namespace backend.Modules.CallOperations.Endpoints
{
    public static class LiveKitWebhookEndpoints
    {
        public static WebApplication MapLiveKitWebhookEndpoints(this WebApplication app)
        {
            app.MapPost("/api/webhooks/livekit", async (
                HttpRequest request, LiveKitService liveKit,
                IMediator mediator) =>
            {
                if (!ServiceAuth.IsConfiguredOrValid(request.HttpContext))
                    return Results.Unauthorized();

                var authHeader = request.Headers["Authorization"].FirstOrDefault();
                using var payload = liveKit.ValidateWebhook(authHeader, "");
                if (payload == null)
                    return Results.Unauthorized();

                var root = payload.RootElement;
                var evt = root.TryGetProperty("event", out var e) ? e.GetString() : null;

                string? roomName = null;
                if (root.TryGetProperty("room", out var roomEl) &&
                    roomEl.ValueKind == JsonValueKind.Object &&
                    roomEl.TryGetProperty("name", out var nameEl))
                    roomName = nameEl.GetString();

                string? identity = null;
                if (root.TryGetProperty("participant", out var pEl) &&
                    pEl.ValueKind == JsonValueKind.Object &&
                    pEl.TryGetProperty("identity", out var idEl))
                    identity = idEl.GetString();

                switch (evt)
                {
                    case "participant_joined":
                        if (roomName != null && identity != null)
                            await mediator.Send(new HandleParticipantJoinedCommand(roomName, identity));
                        break;
                    case "participant_left":
                        if (roomName != null && identity != null)
                            await mediator.Send(new HandleParticipantLeftCommand(roomName, identity));
                        break;
                    case "room_finished":
                        if (roomName != null)
                            await mediator.Send(new HandleRoomFinishedCommand(roomName));
                        break;
                }

                return Results.Ok();
            });

            return app;
        }
    }
}
