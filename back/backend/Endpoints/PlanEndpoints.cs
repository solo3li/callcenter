using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using backend.Dtos;
using backend.Services;

namespace backend.Endpoints
{
    public static class PlanEndpoints
    {
        public static WebApplication MapPlanEndpoints(this WebApplication app)
        {
            app.MapGet("/api/plans", async (PlanService service) =>
            {
                var plans = await service.ListActiveAsync();
                return Results.Ok(plans);
            });

            app.MapGet("/api/plans/all", async (PlanService service) =>
            {
                var plans = await service.ListAsync();
                return Results.Ok(plans);
            });

            app.MapGet("/api/plans/{id:guid}", async (Guid id, PlanService service) =>
            {
                var plan = await service.GetByIdAsync(id);
                return plan == null ? Results.NotFound() : Results.Ok(plan);
            });

            app.MapPost("/api/plans", async (CreatePlanRequest request, PlanService service) =>
            {
                try
                {
                    var plan = await service.CreateAsync(request);
                    return Results.Created($"/api/plans/{plan.Id}", plan);
                }
                catch (ArgumentException ex)
                {
                    return Results.BadRequest(new { error = ex.Message });
                }
            });

            app.MapPut("/api/plans/{id:guid}", async (Guid id, UpdatePlanRequest request, PlanService service) =>
            {
                var plan = await service.UpdateAsync(id, request);
                return plan == null ? Results.NotFound() : Results.Ok(plan);
            });

            app.MapDelete("/api/plans/{id:guid}", async (Guid id, PlanService service) =>
            {
                var deleted = await service.DeleteAsync(id);
                return deleted ? Results.Ok(new { message = "Plan deactivated" }) : Results.NotFound();
            });

            app.MapGet("/api/partners/{partnerId:guid}/plans", async (Guid partnerId, PlanService service) =>
            {
                var plans = await service.ListPartnerPlansAsync(partnerId);
                return Results.Ok(plans);
            });

            app.MapGet("/api/partner-plans/{id:guid}", async (Guid id, PlanService service) =>
            {
                var plan = await service.GetPartnerPlanByIdAsync(id);
                return plan == null ? Results.NotFound() : Results.Ok(plan);
            });

            app.MapPost("/api/partners/{partnerId:guid}/plans", async (Guid partnerId,
                CreatePartnerPlanRequest request, PlanService service) =>
            {
                var plan = await service.CreatePartnerPlanAsync(partnerId, request);
                return Results.Created($"/api/partner-plans/{plan.Id}", plan);
            });

            app.MapDelete("/api/partner-plans/{id:guid}", async (Guid id, PlanService service) =>
            {
                var deleted = await service.DeletePartnerPlanAsync(id);
                return deleted ? Results.Ok(new { message = "Partner plan deactivated" }) : Results.NotFound();
            });

            return app;
        }
    }
}