using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using MediatR;
using backend.Dtos;
using backend.Services;
using backend.Modules.Identity.Features.Partners.ListPartners;
using backend.Modules.Identity.Features.Partners.GetPartner;
using backend.Modules.Identity.Features.Partners.UpdatePartner;
using backend.Modules.Identity.Features.Partners.ListPartnerCustomers;
using backend.Modules.Identity.Features.Partners.AddPartnerCustomer;
using backend.Modules.Identity.Features.Partners.GetPartnerRelationship;
using backend.Modules.Identity.Features.Partners.UpdatePartnerRelationship;
using backend.Modules.Identity.Features.Partners.DeletePartnerRelationship;
using backend.Modules.Identity.Features.Partners.ProvisionCustomer;
using backend.Modules.Identity.Features.Partners.GetProvisionStatus;

namespace backend.Endpoints
{
    public static class PartnerEndpoints
    {
        public static WebApplication MapPartnerEndpoints(this WebApplication app)
        {
            app.MapGet("/api/partners", async (IMediator mediator) =>
            {
                var partners = await mediator.Send(new ListPartnersQuery());
                return Results.Ok(partners);
            });

            app.MapGet("/api/partners/{id:guid}", async (Guid id, IMediator mediator) =>
            {
                var partner = await mediator.Send(new GetPartnerQuery(id));
                return partner == null ? Results.NotFound() : Results.Ok(partner);
            });

            app.MapGet("/api/partners/me", async (HttpContext context, IMediator mediator) =>
            {
                if (!context.Items.TryGetValue("UserId", out var userIdObj) || userIdObj is not Guid userId)
                    return Results.Unauthorized();

                var partner = await mediator.Send(new GetPartnerByUserIdQuery(userId));
                return partner == null ? Results.NotFound() : Results.Ok(partner);
            });

            app.MapPut("/api/partners/{id:guid}", async (Guid id, UpdatePartnerRequest request,
                IMediator mediator) =>
            {
                var partner = await mediator.Send(new UpdatePartnerCommand(id, request));
                return partner == null ? Results.NotFound() : Results.Ok(partner);
            });

            app.MapGet("/api/partners/{partnerId:guid}/customers", async (Guid partnerId,
                IMediator mediator) =>
            {
                var customers = await mediator.Send(new ListPartnerCustomersQuery(partnerId));
                return Results.Ok(customers);
            });

            app.MapPost("/api/partners/{partnerId:guid}/customers", async (Guid partnerId,
                CreateRelationshipRequest request, IMediator mediator) =>
            {
                var rel = await mediator.Send(new AddPartnerCustomerCommand(partnerId, request));
                return Results.Created($"/api/partner-relationships/{rel.Id}", rel);
            });

            app.MapGet("/api/partner-relationships/{id:guid}", async (Guid id, IMediator mediator) =>
            {
                var rel = await mediator.Send(new GetPartnerRelationshipQuery(id));
                return rel == null ? Results.NotFound() : Results.Ok(rel);
            });

            app.MapPut("/api/partner-relationships/{id:guid}", async (Guid id,
                string? status, string? metadataJson, IMediator mediator) =>
            {
                var rel = await mediator.Send(new UpdatePartnerRelationshipCommand(id, status, metadataJson));
                return rel == null ? Results.NotFound() : Results.Ok(rel);
            });

            app.MapDelete("/api/partner-relationships/{id:guid}", async (Guid id, IMediator mediator) =>
            {
                var deleted = await mediator.Send(new DeletePartnerRelationshipCommand(id));
                return deleted ? Results.Ok(new { message = "Relationship deactivated" }) : Results.NotFound();
            });

            app.MapPost("/api/partners/{partnerId:guid}/provision", async (Guid partnerId,
                ProvisionRequest request, IMediator mediator) =>
            {
                try
                {
                    var result = await mediator.Send(new ProvisionCustomerCommand(partnerId, request));
                    return Results.Ok(result);
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(new { error = ex.Message });
                }
            });

            app.MapGet("/api/partners/{partnerId:guid}/provision/{externalCustomerId}", async (
                Guid partnerId, string externalCustomerId, IMediator mediator) =>
            {
                try
                {
                    var status = await mediator.Send(new GetProvisionStatusQuery(partnerId, externalCustomerId));
                    return Results.Ok(status);
                }
                catch (InvalidOperationException)
                {
                    return Results.NotFound();
                }
            });

            app.MapGet("/api/partners/{partnerId:guid}/stats", async (Guid partnerId,
                IMediator mediator) =>
            {
                var partner = await mediator.Send(new GetPartnerQuery(partnerId));
                if (partner == null) return Results.NotFound("Partner not found");

                var stats = await mediator.Send(new backend.Modules.Analytics.Features.Stats.GetSummaryStats.GetSummaryStatsQuery(partner.UserId));
                return Results.Ok(stats);
            });

            return app;
        }
    }
}