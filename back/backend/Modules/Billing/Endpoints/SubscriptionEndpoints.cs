using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using MediatR;
using backend.Modules.Identity.Dtos;
using backend.Modules.Billing.Dtos;
using backend.Modules.CallOperations.Dtos;
using backend.Modules.Configuration.Dtos;
using backend.Modules.Analytics.Dtos;
using backend.Modules.Billing.Features.Subscriptions.ListSubscriptions;
using backend.Modules.Billing.Features.Subscriptions.GetSubscription;
using backend.Modules.Billing.Features.Subscriptions.CreateSubscription;
using backend.Modules.Billing.Features.Subscriptions.UpdateSubscription;
using backend.Modules.Billing.Features.Subscriptions.CancelSubscription;

namespace backend.Modules.Billing.Endpoints
{
    public static class SubscriptionEndpoints
    {
        public static WebApplication MapSubscriptionEndpoints(this WebApplication app)
        {
            app.MapGet("/api/subscriptions", async (HttpContext context, IMediator mediator) =>
            {
                if (!context.Items.TryGetValue("UserId", out var userIdObj) || userIdObj is not Guid userId)
                    return Results.Unauthorized();

                var subs = await mediator.Send(new ListSubscriptionsQuery(userId));
                return Results.Ok(subs);
            });

            app.MapGet("/api/subscriptions/{id:guid}", async (Guid id, IMediator mediator) =>
            {
                var sub = await mediator.Send(new GetSubscriptionQuery(id));
                return sub == null ? Results.NotFound() : Results.Ok(sub);
            });

            app.MapPost("/api/subscriptions", async (HttpContext context,
                CreateSubscriptionRequest request, IMediator mediator) =>
            {
                if (!context.Items.TryGetValue("UserId", out var userIdObj) || userIdObj is not Guid userId)
                    return Results.Unauthorized();

                try
                {
                    var sub = await mediator.Send(new CreateSubscriptionCommand(userId, request));
                    return Results.Created($"/api/subscriptions/{sub.Id}", sub);
                }
                catch (ArgumentException ex)
                {
                    return Results.BadRequest(new { error = ex.Message });
                }
            });

            app.MapPut("/api/subscriptions/{id:guid}", async (Guid id,
                UpdateSubscriptionRequest request, IMediator mediator) =>
            {
                var sub = await mediator.Send(new UpdateSubscriptionCommand(id, request));
                return sub == null ? Results.NotFound() : Results.Ok(sub);
            });

            app.MapPost("/api/subscriptions/{id:guid}/cancel", async (Guid id, IMediator mediator) =>
            {
                var cancelled = await mediator.Send(new CancelSubscriptionCommand(id));
                return cancelled ? Results.Ok(new { message = "Subscription cancelled" }) : Results.NotFound();
            });

            return app;
        }
    }
}