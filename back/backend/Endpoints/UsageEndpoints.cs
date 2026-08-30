using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using MediatR;
using backend.Data;
using backend.Dtos;
using backend.Modules.Billing.Features.Usage.ListUsage;
using backend.Modules.Billing.Features.Usage.GetUsageSummary;
using backend.Modules.Billing.Features.Usage.GetUsageByMetricType;
using backend.Modules.Billing.Features.Usage.GetUsageByCall;
using backend.Modules.Billing.Features.Usage.RecordUsage;

namespace backend.Endpoints
{
    public static class UsageEndpoints
    {
        public static WebApplication MapUsageEndpoints(this WebApplication app)
        {
            app.MapGet("/api/usage", async (HttpContext context, IMediator mediator, Guid? callSessionId,
                Guid? licenseId, Guid? partnerId, string? metricType, DateTime? from, DateTime? to) =>
            {
                if (!context.Items.TryGetValue("UserId", out var userIdObj) || userIdObj is not Guid userId)
                    return Results.Unauthorized();

                var filter = new UsageFilterRequest(from, to, metricType, callSessionId, licenseId, partnerId);
                var records = await mediator.Send(new ListUsageQuery(userId, filter));
                return Results.Ok(records);
            });

            app.MapGet("/api/usage/summary", async (HttpContext context, IMediator mediator) =>
            {
                if (!context.Items.TryGetValue("UserId", out var userIdObj) || userIdObj is not Guid userId)
                    return Results.Unauthorized();

                var summary = await mediator.Send(new GetUsageSummaryQuery(userId));
                return Results.Ok(summary);
            });

            app.MapGet("/api/usage/metric/{metricType}", async (HttpContext context, string metricType,
                IMediator mediator) =>
            {
                if (!context.Items.TryGetValue("UserId", out var userIdObj) || userIdObj is not Guid userId)
                    return Results.Unauthorized();

                var records = await mediator.Send(new GetUsageByMetricTypeQuery(userId, metricType));
                return Results.Ok(records);
            });

            app.MapGet("/api/usage/call/{callSessionId:guid}", async (HttpContext context, Guid callSessionId,
                IMediator mediator) =>
            {
                if (!context.Items.TryGetValue("UserId", out var userIdObj) || userIdObj is not Guid userId)
                    return Results.Unauthorized();

                var records = await mediator.Send(new GetUsageByCallQuery(userId, callSessionId));
                return Results.Ok(records);
            });

            app.MapPost("/api/usage", async (HttpContext context, IMediator mediator,
                AppDbContext db, Guid? partnerId, Guid? licenseId, Guid? callSessionId,
                string metricType, decimal quantity, string unit) =>
            {
                Guid userId;

                if (context.Items.TryGetValue("UserId", out var userIdObj) && userIdObj is Guid jwtUser)
                {
                    userId = jwtUser;
                }
                else if (backend.Middleware.ServiceAuth.IsConfiguredOrValid(context)
                    && callSessionId.HasValue)
                {
                    // Worker path: derive ownership from the call session itself.
                    var owner = await db.CallSessions
                        .Where(c => c.Id == callSessionId.Value)
                        .Select(c => (Guid?)c.UserId)
                        .FirstOrDefaultAsync();
                    if (owner == null)
                        return Results.NotFound(new { error = "Call session not found" });
                    userId = owner.Value;
                }
                else
                {
                    return Results.Unauthorized();
                }

                try
                {
                    var record = await mediator.Send(new RecordUsageCommand(
                        userId, partnerId, licenseId, callSessionId, metricType, quantity, unit));
                    return Results.Ok(record);
                }
                catch (ArgumentException ex)
                {
                    return Results.BadRequest(new { error = ex.Message });
                }
            });

            return app;
        }
    }
}