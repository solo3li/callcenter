using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace backend.Endpoints
{
    public static class WebhookEndpoints
    {
        public static WebApplication MapWebhookEndpoints(this WebApplication app)
        {
            app.MapPost("/api/webhooks/recording-complete", async () =>
            {
                await System.Threading.Tasks.Task.CompletedTask;
                return Results.Ok(new { status = "received", webhook = "recording-complete" });
            });

            app.MapPost("/api/webhooks/call-started", async () =>
            {
                await System.Threading.Tasks.Task.CompletedTask;
                return Results.Ok(new { status = "received", webhook = "call-started" });
            });

            app.MapPost("/api/webhooks/call-ended", async () =>
            {
                await System.Threading.Tasks.Task.CompletedTask;
                return Results.Ok(new { status = "received", webhook = "call-ended" });
            });

            return app;
        }
    }
}