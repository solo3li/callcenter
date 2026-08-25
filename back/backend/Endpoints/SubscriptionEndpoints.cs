using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using backend.Dtos;
using backend.Services;

namespace backend.Endpoints
{
    public static class SubscriptionEndpoints
    {
        public static WebApplication MapSubscriptionEndpoints(this WebApplication app)
        {
            app.MapGet("/api/subscriptions", async (HttpContext context, SubscriptionService service) =>
            {
                if (!context.Items.TryGetValue("UserId", out var userIdObj) || userIdObj is not Guid userId)
                    return Results.Unauthorized();

                var subs = await service.ListAsync(userId);
                return Results.Ok(subs);
            });

            app.MapGet("/api/subscriptions/{id:guid}", async (Guid id, SubscriptionService service) =>
            {
                var sub = await service.GetByIdAsync(id);
                return sub == null ? Results.NotFound() : Results.Ok(sub);
            });

            app.MapPost("/api/subscriptions", async (HttpContext context,
                CreateSubscriptionRequest request, SubscriptionService service) =>
            {
                if (!context.Items.TryGetValue("UserId", out var userIdObj) || userIdObj is not Guid userId)
                    return Results.Unauthorized();

                try
                {
                    var sub = await service.CreateAsync(userId, request);
                    return Results.Created($"/api/subscriptions/{sub.Id}", sub);
                }
                catch (ArgumentException ex)
                {
                    return Results.BadRequest(new { error = ex.Message });
                }
            });

            app.MapPut("/api/subscriptions/{id:guid}", async (Guid id,
                UpdateSubscriptionRequest request, SubscriptionService service) =>
            {
                var sub = await service.UpdateAsync(id, request);
                return sub == null ? Results.NotFound() : Results.Ok(sub);
            });

            app.MapPost("/api/subscriptions/{id:guid}/cancel", async (Guid id,
                SubscriptionService service) =>
            {
                var cancelled = await service.CancelAsync(id);
                return cancelled ? Results.Ok(new { message = "Subscription cancelled" }) : Results.NotFound();
            });

            return app;
        }
    }
}