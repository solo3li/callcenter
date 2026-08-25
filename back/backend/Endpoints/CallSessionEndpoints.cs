using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using backend.Dtos;
using backend.Services;

namespace backend.Endpoints
{
    public static class CallSessionEndpoints
    {
        public static void MapCallSessionEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/calls");

            group.MapGet("/", async (
                string? status,
                string? direction,
                DateTime? from,
                DateTime? to,
                int page = 1,
                int limit = 20,
                CallSessionService service = null!,
                HttpContext http = null!) =>
            {
                var userId = (Guid)http.Items["UserId"]!;
                var (items, totalCount) = await service.ListAsync(userId, status, direction, from, to, page, limit);
                return Results.Ok(new { items, totalCount, page, limit });
            });

            group.MapGet("/{id:guid}", async (
                Guid id,
                CallSessionService service,
                HttpContext http) =>
            {
                var userId = (Guid)http.Items["UserId"]!;
                var result = await service.GetByIdAsync(id, userId);
                return result is not null ? Results.Ok(result) : Results.NotFound();
            });

            group.MapGet("/active", async (
                CallSessionService service,
                HttpContext http) =>
            {
                var userId = (Guid)http.Items["UserId"]!;
                var result = await service.GetActiveAsync(userId);
                return Results.Ok(result);
            });

            group.MapPost("/{id:guid}/end", async (
                Guid id,
                CallSessionService service,
                HttpContext http) =>
            {
                var userId = (Guid)http.Items["UserId"]!;
                var result = await service.EndCallAsync(id, userId);
                return result is not null ? Results.Ok(result) : Results.NotFound();
            });

            group.MapPatch("/{id:guid}/metadata", async (
                Guid id,
                [FromBody] UpdateMetadataRequest request,
                CallSessionService service,
                HttpContext http) =>
            {
                var userId = (Guid)http.Items["UserId"]!;
                var updated = await service.UpdateMetadataAsync(id, userId, request.MetadataJson);
                return updated ? Results.Ok() : Results.NotFound();
            });

            group.MapGet("/{id:guid}/participants", async (
                Guid id,
                CallSessionService service,
                HttpContext http) =>
            {
                var userId = (Guid)http.Items["UserId"]!;
                var result = await service.GetParticipantsAsync(id, userId);
                return Results.Ok(result);
            });

            group.MapGet("/{id:guid}/participants/{pid:guid}", async (
                Guid id,
                Guid pid,
                CallSessionService service,
                HttpContext http) =>
            {
                var userId = (Guid)http.Items["UserId"]!;
                var result = await service.GetParticipantByIdAsync(id, pid, userId);
                return result is not null ? Results.Ok(result) : Results.NotFound();
            });
        }
    }
}