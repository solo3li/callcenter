using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using MediatR;
using backend.Dtos;
using backend.Modules.Identity.Features.Licenses.ListLicenses;
using backend.Modules.Identity.Features.Licenses.GetLicense;
using backend.Modules.Identity.Features.Licenses.CreateLicense;
using backend.Modules.Identity.Features.Licenses.UpdateLicense;
using backend.Modules.Identity.Features.Licenses.DeleteLicense;

namespace backend.Endpoints
{
    public static class LicenseEndpoints
    {
        public static WebApplication MapLicenseEndpoints(this WebApplication app)
        {
            app.MapGet("/api/licenses", async (HttpContext context, IMediator mediator,
                Guid? partnerId) =>
            {
                if (!context.Items.TryGetValue("UserId", out var userIdObj) || userIdObj is not Guid userId)
                    return Results.Unauthorized();

                var licenses = await mediator.Send(new ListLicensesQuery(userId, partnerId));
                return Results.Ok(licenses);
            });

            app.MapGet("/api/licenses/{id:guid}", async (Guid id, IMediator mediator) =>
            {
                var license = await mediator.Send(new GetLicenseQuery(id));
                return license == null ? Results.NotFound() : Results.Ok(license);
            });

            app.MapPost("/api/licenses", async (CreateLicenseRequest request, IMediator mediator) =>
            {
                var license = await mediator.Send(new CreateLicenseCommand(request));
                return Results.Created($"/api/licenses/{license.Id}", license);
            });

            app.MapPut("/api/licenses/{id:guid}", async (Guid id,
                UpdateLicenseRequest request, IMediator mediator) =>
            {
                var license = await mediator.Send(new UpdateLicenseCommand(id, request));
                return license == null ? Results.NotFound() : Results.Ok(license);
            });

            app.MapDelete("/api/licenses/{id:guid}", async (Guid id, IMediator mediator) =>
            {
                var deleted = await mediator.Send(new DeleteLicenseCommand(id));
                return deleted ? Results.Ok(new { message = "License cancelled" }) : Results.NotFound();
            });

            return app;
        }
    }
}