using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using backend.Dtos;
using backend.Services;
using MediatR;

namespace backend.Endpoints
{
    public static class CallRecordingEndpoints
    {
        public static void MapCallRecordingEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/calls/{callSessionId:guid}/recordings");

            group.MapGet("/", async (
                Guid callSessionId,
                IMediator mediator) =>
            {
                var result = await mediator.Send(new backend.Modules.CallOperations.Features.CallRecordings.ListCallRecordings.ListCallRecordingsQuery(callSessionId));
                return Results.Ok(result);
            });

            group.MapGet("/{recordingId:guid}", async (
                Guid callSessionId,
                Guid recordingId,
                IMediator mediator) =>
            {
                var result = await mediator.Send(new backend.Modules.CallOperations.Features.CallRecordings.GetCallRecording.GetCallRecordingQuery(recordingId));
                return result is not null ? Results.Ok(result) : Results.NotFound();
            });

            group.MapGet("/{recordingId:guid}/download", async (
                Guid callSessionId,
                Guid recordingId,
                IMediator mediator) =>
            {
                var result = await mediator.Send(new backend.Modules.CallOperations.Features.CallRecordings.GetCallRecordingDownloadUrl.GetCallRecordingDownloadUrlQuery(recordingId));
                return Results.Ok(result);
            });

            group.MapPost("/", async (
                Guid callSessionId,
                [FromBody] RecordingCallbackRequest request,
                IMediator mediator) =>
            {
                var result = await mediator.Send(new backend.Modules.CallOperations.Features.CallRecordings.HandleRecordingCallback.HandleRecordingCallbackCommand(callSessionId, request));
                return Results.Created($"/api/calls/{callSessionId}/recordings/{result.Id}", result);
            });

            group.MapDelete("/{recordingId:guid}", async (
                Guid callSessionId,
                Guid recordingId,
                IMediator mediator) =>
            {
                var deleted = await mediator.Send(new backend.Modules.CallOperations.Features.CallRecordings.DeleteCallRecording.DeleteCallRecordingCommand(recordingId));
                return deleted ? Results.NoContent() : Results.NotFound();
            });
        }
    }
}
