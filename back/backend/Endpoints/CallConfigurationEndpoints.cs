using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using backend.Dtos;
using backend.Services;

namespace backend.Endpoints
{
    public static class CallConfigurationEndpoints
    {
        public static void MapCallConfigurationEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/call-configurations");

            group.MapGet("/", async (
                CallConfigurationService service,
                HttpContext http) =>
            {
                var userId = (Guid)http.Items["UserId"]!;
                var result = await service.ListAsync(userId);
                return Results.Ok(result);
            });

            group.MapGet("/{id:guid}", async (
                Guid id,
                CallConfigurationService service,
                HttpContext http) =>
            {
                var userId = (Guid)http.Items["UserId"]!;
                var result = await service.GetByIdAsync(id, userId);
                return result is not null ? Results.Ok(result) : Results.NotFound();
            });

            group.MapPost("/", async (
                [FromBody] CreateCallConfigRequest request,
                CallConfigurationService service,
                HttpContext http) =>
            {
                var userId = (Guid)http.Items["UserId"]!;
                var result = await service.CreateAsync(userId, request);
                return Results.Created($"/api/call-configurations/{result.Id}", result);
            });

            group.MapPatch("/{id:guid}", async (
                Guid id,
                [FromBody] UpdateCallConfigRequest request,
                CallConfigurationService service,
                HttpContext http) =>
            {
                var userId = (Guid)http.Items["UserId"]!;
                var result = await service.UpdateAsync(id, userId, request);
                return result is not null ? Results.Ok(result) : Results.NotFound();
            });

            group.MapDelete("/{id:guid}", async (
                Guid id,
                CallConfigurationService service,
                HttpContext http) =>
            {
                var userId = (Guid)http.Items["UserId"]!;
                var deleted = await service.DeleteAsync(id, userId);
                return deleted ? Results.NoContent() : Results.NotFound();
            });

            group.MapPost("/{id:guid}/activate", async (
                Guid id,
                CallConfigurationService service,
                HttpContext http) =>
            {
                var userId = (Guid)http.Items["UserId"]!;
                var result = await service.ActivateAsync(id, userId);
                return result is not null ? Results.Ok(result) : Results.NotFound();
            });

            group.MapPut("/{id:guid}/actions", async (
                Guid id,
                [FromBody] SetConfigActionsRequest request,
                CallConfigurationService service,
                HttpContext http) =>
            {
                var userId = (Guid)http.Items["UserId"]!;
                var result = await service.SetActionsAsync(id, userId, request);
                return result is not null ? Results.Ok(result) : Results.NotFound();
            });

            group.MapGet("/{id:guid}/actions", async (
                Guid id,
                CallConfigurationService service,
                HttpContext http) =>
            {
                var userId = (Guid)http.Items["UserId"]!;
                var result = await service.GetActionsAsync(id, userId);
                return Results.Ok(result);
            });
        }
    }
}