using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using backend.Dtos;
using backend.Services;

namespace backend.Endpoints
{
    public static class CallRecordingEndpoints
    {
        public static void MapCallRecordingEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/calls/{callSessionId:guid}/recordings");

            group.MapGet("/", async (
                Guid callSessionId,
                CallRecordingService service) =>
            {
                var result = await service.ListForCallAsync(callSessionId);
                return Results.Ok(result);
            });

            group.MapGet("/{recordingId:guid}", async (
                Guid callSessionId,
                Guid recordingId,
                CallRecordingService service) =>
            {
                var result = await service.GetByIdAsync(recordingId);
                return result is not null ? Results.Ok(result) : Results.NotFound();
            });

            group.MapGet("/{recordingId:guid}/download", async (
                Guid callSessionId,
                Guid recordingId,
                CallRecordingService service) =>
            {
                var result = await service.GenerateDownloadUrl(recordingId);
                return Results.Ok(result);
            });

            group.MapPost("/", async (
                Guid callSessionId,
                [FromBody] RecordingCallbackRequest request,
                CallRecordingService service) =>
            {
                var result = await service.HandleEgressCallback(callSessionId, request);
                return Results.Created($"/api/calls/{callSessionId}/recordings/{result.Id}", result);
            });

            group.MapDelete("/{recordingId:guid}", async (
                Guid callSessionId,
                Guid recordingId,
                CallRecordingService service) =>
            {
                var deleted = await service.DeleteAsync(recordingId);
                return deleted ? Results.NoContent() : Results.NotFound();
            });
        }
    }
}