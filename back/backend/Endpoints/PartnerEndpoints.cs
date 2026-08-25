using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using backend.Dtos;
using backend.Services;

namespace backend.Endpoints
{
    public static class PartnerEndpoints
    {
        public static WebApplication MapPartnerEndpoints(this WebApplication app)
        {
            app.MapGet("/api/partners", async (PartnerService service) =>
            {
                var partners = await service.ListAsync();
                return Results.Ok(partners);
            });

            app.MapGet("/api/partners/{id:guid}", async (Guid id, PartnerService service) =>
            {
                var partner = await service.GetByIdAsync(id);
                return partner == null ? Results.NotFound() : Results.Ok(partner);
            });

            app.MapGet("/api/partners/me", async (HttpContext context, PartnerService service) =>
            {
                if (!context.Items.TryGetValue("UserId", out var userIdObj) || userIdObj is not Guid userId)
                    return Results.Unauthorized();

                var partner = await service.GetByUserIdAsync(userId);
                return partner == null ? Results.NotFound() : Results.Ok(partner);
            });

            app.MapPut("/api/partners/{id:guid}", async (Guid id, UpdatePartnerRequest request,
                PartnerService service) =>
            {
                var partner = await service.UpdateAsync(id, request);
                return partner == null ? Results.NotFound() : Results.Ok(partner);
            });

            app.MapGet("/api/partners/{partnerId:guid}/customers", async (Guid partnerId,
                PartnerService service) =>
            {
                var customers = await service.ListCustomersAsync(partnerId);
                return Results.Ok(customers);
            });

            app.MapPost("/api/partners/{partnerId:guid}/customers", async (Guid partnerId,
                CreateRelationshipRequest request, PartnerService service) =>
            {
                var rel = await service.AddCustomerAsync(partnerId, request);
                return Results.Created($"/api/partner-relationships/{rel.Id}", rel);
            });

            app.MapGet("/api/partner-relationships/{id:guid}", async (Guid id, PartnerService service) =>
            {
                var rel = await service.GetRelationshipAsync(id);
                return rel == null ? Results.NotFound() : Results.Ok(rel);
            });

            app.MapPut("/api/partner-relationships/{id:guid}", async (Guid id,
                string? status, string? metadataJson, PartnerService service) =>
            {
                var rel = await service.UpdateRelationshipAsync(id, status, metadataJson);
                return rel == null ? Results.NotFound() : Results.Ok(rel);
            });

            app.MapDelete("/api/partner-relationships/{id:guid}", async (Guid id, PartnerService service) =>
            {
                var deleted = await service.DeleteRelationshipAsync(id);
                return deleted ? Results.Ok(new { message = "Relationship deactivated" }) : Results.NotFound();
            });

            app.MapPost("/api/partners/{partnerId:guid}/provision", async (Guid partnerId,
                ProvisionRequest request, PartnerService service) =>
            {
                try
                {
                    var result = await service.ProvisionCustomerAsync(partnerId, request);
                    return Results.Ok(result);
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(new { error = ex.Message });
                }
            });

            app.MapGet("/api/partners/{partnerId:guid}/provision/{externalCustomerId}", async (
                Guid partnerId, string externalCustomerId, PartnerService service) =>
            {
                try
                {
                    var status = await service.GetProvisionStatusAsync(partnerId, externalCustomerId);
                    return Results.Ok(status);
                }
                catch (InvalidOperationException)
                {
                    return Results.NotFound();
                }
            });

            app.MapGet("/api/partners/{partnerId:guid}/stats", async (Guid partnerId,
                PartnerService service, StatsService statsService) =>
            {
                var partner = await service.GetByIdAsync(partnerId);
                if (partner == null) return Results.NotFound("Partner not found");

                var stats = await statsService.GetSummaryStatsAsync(partner.UserId);
                return Results.Ok(stats);
            });

            return app;
        }
    }
}