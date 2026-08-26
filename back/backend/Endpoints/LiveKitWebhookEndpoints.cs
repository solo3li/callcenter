using System.Text.Json;
using backend.Middleware;
using backend.Services;

namespace backend.Endpoints
{
    public static class LiveKitWebhookEndpoints
    {
        public static WebApplication MapLiveKitWebhookEndpoints(this WebApplication app)
        {
            app.MapPost("/api/webhooks/livekit", async (
                HttpRequest request, LiveKitService liveKit,
                InboundRoutingService routing) =>
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
                            await routing.HandleParticipantJoinedAsync(roomName, identity);
                        break;
                    case "participant_left":
                        if (roomName != null && identity != null)
                            await routing.HandleParticipantLeftAsync(roomName, identity);
                        break;
                    case "room_finished":
                        if (roomName != null)
                            await routing.HandleRoomFinishedAsync(roomName);
                        break;
                }

                return Results.Ok();
            });

            return app;
        }
    }
}
