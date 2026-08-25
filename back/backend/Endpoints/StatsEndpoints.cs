using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using backend.Services;

namespace backend.Endpoints
{
    public static class StatsEndpoints
    {
        public static WebApplication MapStatsEndpoints(this WebApplication app)
        {
            app.MapGet("/api/stats/today", async (HttpContext context, StatsService service) =>
            {
                if (!context.Items.TryGetValue("UserId", out var userIdObj) || userIdObj is not Guid userId)
                    return Results.Unauthorized();

                var stats = await service.GetTodayStatsAsync(userId);
                return Results.Ok(stats);
            });

            app.MapGet("/api/stats/queue", async (HttpContext context, StatsService service) =>
            {
                if (!context.Items.TryGetValue("UserId", out var userIdObj) || userIdObj is not Guid userId)
                    return Results.Unauthorized();

                var stats = await service.GetQueueStatsAsync(userId);
                return Results.Ok(stats);
            });

            app.MapGet("/api/stats/agents", async (HttpContext context, StatsService service) =>
            {
                if (!context.Items.TryGetValue("UserId", out var userIdObj) || userIdObj is not Guid userId)
                    return Results.Unauthorized();

                var stats = await service.GetAgentStatsAsync(userId);
                return Results.Ok(stats);
            });

            app.MapGet("/api/stats/period", async (HttpContext context, StatsService service,
                DateTime? from, DateTime? to) =>
            {
                if (!context.Items.TryGetValue("UserId", out var userIdObj) || userIdObj is not Guid userId)
                    return Results.Unauthorized();

                var f = from ?? DateTime.UtcNow.AddDays(-7);
                var t = to ?? DateTime.UtcNow;
                var stats = await service.GetPeriodStatsAsync(userId, f, t);
                return Results.Ok(stats);
            });

            app.MapGet("/api/stats/summary", async (HttpContext context, StatsService service) =>
            {
                if (!context.Items.TryGetValue("UserId", out var userIdObj) || userIdObj is not Guid userId)
                    return Results.Unauthorized();

                var stats = await service.GetSummaryStatsAsync(userId);
                return Results.Ok(stats);
            });

            app.MapGet("/api/stats/hourly", async (HttpContext context, StatsService service, DateTime? date) =>
            {
                if (!context.Items.TryGetValue("UserId", out var userIdObj) || userIdObj is not Guid userId)
                    return Results.Unauthorized();

                var stats = await service.GetHourlyStatsAsync(userId, date);
                return Results.Ok(stats);
            });

            app.MapGet("/api/stats/intents", async (HttpContext context, StatsService service,
                DateTime? from, DateTime? to) =>
            {
                if (!context.Items.TryGetValue("UserId", out var userIdObj) || userIdObj is not Guid userId)
                    return Results.Unauthorized();

                var stats = await service.GetIntentStatsAsync(userId, from, to);
                return Results.Ok(stats);
            });

            app.MapGet("/api/health", async (StatsService service) =>
            {
                var health = await service.GetHealthAsync();
                return Results.Ok(health);
            });

            return app;
        }
    }
}