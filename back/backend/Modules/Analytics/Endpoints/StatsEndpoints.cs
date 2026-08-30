using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using MediatR;
using backend.Modules.Analytics.Features.Stats.GetTodayStats;
using backend.Modules.Analytics.Features.Stats.GetQueueStats;
using backend.Modules.Analytics.Features.Stats.GetAgentStats;
using backend.Modules.Analytics.Features.Stats.GetPeriodStats;
using backend.Modules.Analytics.Features.Stats.GetSummaryStats;
using backend.Modules.Analytics.Features.Stats.GetHourlyStats;
using backend.Modules.Analytics.Features.Stats.GetIntentStats;
using backend.Modules.Analytics.Features.Stats.GetHealthStats;

namespace backend.Modules.Analytics.Endpoints
{
    public static class StatsEndpoints
    {
        public static WebApplication MapStatsEndpoints(this WebApplication app)
        {
            app.MapGet("/api/stats/today", async (HttpContext context, IMediator mediator) =>
            {
                if (!context.Items.TryGetValue("UserId", out var userIdObj) || userIdObj is not Guid userId)
                    return Results.Unauthorized();

                var stats = await mediator.Send(new GetTodayStatsQuery(userId));
                return Results.Ok(stats);
            });

            app.MapGet("/api/stats/queue", async (HttpContext context, IMediator mediator) =>
            {
                if (!context.Items.TryGetValue("UserId", out var userIdObj) || userIdObj is not Guid userId)
                    return Results.Unauthorized();

                var stats = await mediator.Send(new GetQueueStatsQuery(userId));
                return Results.Ok(stats);
            });

            app.MapGet("/api/stats/agents", async (HttpContext context, IMediator mediator) =>
            {
                if (!context.Items.TryGetValue("UserId", out var userIdObj) || userIdObj is not Guid userId)
                    return Results.Unauthorized();

                var stats = await mediator.Send(new GetAgentStatsQuery(userId));
                return Results.Ok(stats);
            });

            app.MapGet("/api/stats/period", async (HttpContext context, IMediator mediator,
                DateTime? from, DateTime? to) =>
            {
                if (!context.Items.TryGetValue("UserId", out var userIdObj) || userIdObj is not Guid userId)
                    return Results.Unauthorized();

                var f = from ?? DateTime.UtcNow.AddDays(-7);
                var t = to ?? DateTime.UtcNow;
                var stats = await mediator.Send(new GetPeriodStatsQuery(userId, f, t));
                return Results.Ok(stats);
            });

            app.MapGet("/api/stats/summary", async (HttpContext context, IMediator mediator) =>
            {
                if (!context.Items.TryGetValue("UserId", out var userIdObj) || userIdObj is not Guid userId)
                    return Results.Unauthorized();

                var stats = await mediator.Send(new GetSummaryStatsQuery(userId));
                return Results.Ok(stats);
            });

            app.MapGet("/api/stats/hourly", async (HttpContext context, IMediator mediator, DateTime? date) =>
            {
                if (!context.Items.TryGetValue("UserId", out var userIdObj) || userIdObj is not Guid userId)
                    return Results.Unauthorized();

                var stats = await mediator.Send(new GetHourlyStatsQuery(userId, date));
                return Results.Ok(stats);
            });

            app.MapGet("/api/stats/intents", async (HttpContext context, IMediator mediator,
                DateTime? from, DateTime? to) =>
            {
                if (!context.Items.TryGetValue("UserId", out var userIdObj) || userIdObj is not Guid userId)
                    return Results.Unauthorized();

                var stats = await mediator.Send(new GetIntentStatsQuery(userId, from, to));
                return Results.Ok(stats);
            });

            app.MapGet("/api/health", async (IMediator mediator) =>
            {
                var health = await mediator.Send(new GetHealthStatsQuery());
                return Results.Ok(health);
            });

            return app;
        }
    }
}