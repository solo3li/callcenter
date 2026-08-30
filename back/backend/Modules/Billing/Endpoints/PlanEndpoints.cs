using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using MediatR;
using backend.Modules.Identity.Dtos;
using backend.Modules.Billing.Dtos;
using backend.Modules.CallOperations.Dtos;
using backend.Modules.Configuration.Dtos;
using backend.Modules.Analytics.Dtos;
using backend.Modules.Billing.Features.Plans.ListPlans;
using backend.Modules.Billing.Features.Plans.GetPlan;
using backend.Modules.Billing.Features.Plans.CreatePlan;
using backend.Modules.Billing.Features.Plans.UpdatePlan;
using backend.Modules.Billing.Features.Plans.DeletePlan;
using backend.Modules.Identity.Features.PartnerPlans.ListPartnerPlans;
using backend.Modules.Identity.Features.PartnerPlans.GetPartnerPlan;
using backend.Modules.Identity.Features.PartnerPlans.CreatePartnerPlan;
using backend.Modules.Identity.Features.PartnerPlans.DeletePartnerPlan;

namespace backend.Modules.Billing.Endpoints
{
    public static class PlanEndpoints
    {
        public static WebApplication MapPlanEndpoints(this WebApplication app)
        {
            app.MapGet("/api/plans", async (IMediator mediator) =>
            {
                var plans = await mediator.Send(new ListPlansQuery(OnlyActive: true));
                return Results.Ok(plans);
            });

            app.MapGet("/api/plans/all", async (IMediator mediator) =>
            {
                var plans = await mediator.Send(new ListPlansQuery(OnlyActive: false));
                return Results.Ok(plans);
            });

            app.MapGet("/api/plans/{id:guid}", async (Guid id, IMediator mediator) =>
            {
                var plan = await mediator.Send(new GetPlanQuery(id));
                return plan == null ? Results.NotFound() : Results.Ok(plan);
            });

            app.MapPost("/api/plans", async (CreatePlanRequest request, IMediator mediator) =>
            {
                try
                {
                    var plan = await mediator.Send(new CreatePlanCommand(request));
                    return Results.Created($"/api/plans/{plan.Id}", plan);
                }
                catch (ArgumentException ex)
                {
                    return Results.BadRequest(new { error = ex.Message });
                }
            });

            app.MapPut("/api/plans/{id:guid}", async (Guid id, UpdatePlanRequest request, IMediator mediator) =>
            {
                var plan = await mediator.Send(new UpdatePlanCommand(id, request));
                return plan == null ? Results.NotFound() : Results.Ok(plan);
            });

            app.MapDelete("/api/plans/{id:guid}", async (Guid id, IMediator mediator) =>
            {
                var deleted = await mediator.Send(new DeletePlanCommand(id));
                return deleted ? Results.Ok(new { message = "Plan deactivated" }) : Results.NotFound();
            });

            app.MapGet("/api/partners/{partnerId:guid}/plans", async (Guid partnerId, IMediator mediator) =>
            {
                var plans = await mediator.Send(new ListPartnerPlansQuery(partnerId));
                return Results.Ok(plans);
            });

            app.MapGet("/api/partner-plans/{id:guid}", async (Guid id, IMediator mediator) =>
            {
                var plan = await mediator.Send(new GetPartnerPlanQuery(id));
                return plan == null ? Results.NotFound() : Results.Ok(plan);
            });

            app.MapPost("/api/partners/{partnerId:guid}/plans", async (Guid partnerId,
                CreatePartnerPlanRequest request, IMediator mediator) =>
            {
                var plan = await mediator.Send(new CreatePartnerPlanCommand(partnerId, request));
                return Results.Created($"/api/partner-plans/{plan.Id}", plan);
            });

            app.MapDelete("/api/partner-plans/{id:guid}", async (Guid id, IMediator mediator) =>
            {
                var deleted = await mediator.Send(new DeletePartnerPlanCommand(id));
                return deleted ? Results.Ok(new { message = "Partner plan deactivated" }) : Results.NotFound();
            });

            return app;
        }
    }
}