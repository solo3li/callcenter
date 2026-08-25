using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using backend.Dtos;
using backend.Services;

namespace backend.Endpoints
{
    public static class LicenseEndpoints
    {
        public static WebApplication MapLicenseEndpoints(this WebApplication app)
        {
            app.MapGet("/api/licenses", async (HttpContext context, LicenseService service,
                Guid? partnerId) =>
            {
                if (!context.Items.TryGetValue("UserId", out var userIdObj) || userIdObj is not Guid userId)
                    return Results.Unauthorized();

                var licenses = await service.ListAsync(userId, partnerId);
                return Results.Ok(licenses);
            });

            app.MapGet("/api/licenses/{id:guid}", async (Guid id, LicenseService service) =>
            {
                var license = await service.GetByIdAsync(id);
                return license == null ? Results.NotFound() : Results.Ok(license);
            });

            app.MapPost("/api/licenses", async (CreateLicenseRequest request, LicenseService service) =>
            {
                var license = await service.CreateAsync(request);
                return Results.Created($"/api/licenses/{license.Id}", license);
            });

            app.MapPut("/api/licenses/{id:guid}", async (Guid id,
                UpdateLicenseRequest request, LicenseService service) =>
            {
                var license = await service.UpdateAsync(id, request);
                return license == null ? Results.NotFound() : Results.Ok(license);
            });

            app.MapDelete("/api/licenses/{id:guid}", async (Guid id, LicenseService service) =>
            {
                var deleted = await service.DeleteAsync(id);
                return deleted ? Results.Ok(new { message = "License cancelled" }) : Results.NotFound();
            });

            return app;
        }
    }
}