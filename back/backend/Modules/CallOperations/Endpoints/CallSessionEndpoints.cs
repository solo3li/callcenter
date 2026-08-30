using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using backend.Modules.Identity.Dtos;
using backend.Modules.Billing.Dtos;
using backend.Modules.CallOperations.Dtos;
using backend.Modules.Configuration.Dtos;
using backend.Modules.Analytics.Dtos;
using backend.Modules.CallOperations.Features.CallSessions.ListCallSessions;
using backend.Modules.CallOperations.Features.CallSessions.GetCallSession;
using backend.Modules.CallOperations.Features.CallSessions.GetActiveCalls;
using backend.Modules.CallOperations.Features.CallSessions.EndCallSession;
using backend.Modules.CallOperations.Features.CallSessions.UpdateCallSessionMetadata;
using backend.Modules.CallOperations.Features.CallSessions.ListCallParticipants;
using backend.Modules.CallOperations.Features.CallSessions.GetCallParticipant;

namespace backend.Modules.CallOperations.Endpoints
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
                IMediator mediator = null!,
                HttpContext http = null!) =>
            {
                var userId = (Guid)http.Items["UserId"]!;
                var (items, totalCount) = await mediator.Send(new ListCallSessionsQuery(userId, status, direction, from, to, page, limit));
                return Results.Ok(new { items, totalCount, page, limit });
            });

            group.MapGet("/{id:guid}", async (
                Guid id,
                IMediator mediator,
                HttpContext http) =>
            {
                var userId = (Guid)http.Items["UserId"]!;
                var result = await mediator.Send(new GetCallSessionQuery(id, userId));
                return result is not null ? Results.Ok(result) : Results.NotFound();
            });

            group.MapGet("/active", async (
                IMediator mediator,
                HttpContext http) =>
            {
                var userId = (Guid)http.Items["UserId"]!;
                var result = await mediator.Send(new GetActiveCallsQuery(userId));
                return Results.Ok(result);
            });

            group.MapPost("/{id:guid}/end", async (
                Guid id,
                IMediator mediator,
                HttpContext http) =>
            {
                var userId = (Guid)http.Items["UserId"]!;
                var result = await mediator.Send(new EndCallSessionCommand(id, userId));
                return result is not null ? Results.Ok(result) : Results.NotFound();
            });

            group.MapPatch("/{id:guid}/metadata", async (
                Guid id,
                [FromBody] UpdateMetadataRequest request,
                IMediator mediator,
                HttpContext http) =>
            {
                var userId = (Guid)http.Items["UserId"]!;
                var updated = await mediator.Send(new UpdateCallSessionMetadataCommand(id, userId, request.MetadataJson));
                return updated ? Results.Ok() : Results.NotFound();
            });

            group.MapGet("/{id:guid}/participants", async (
                Guid id,
                IMediator mediator,
                HttpContext http) =>
            {
                var userId = (Guid)http.Items["UserId"]!;
                var result = await mediator.Send(new ListCallParticipantsQuery(id, userId));
                return Results.Ok(result);
            });

            group.MapGet("/{id:guid}/participants/{pid:guid}", async (
                Guid id,
                Guid pid,
                IMediator mediator,
                HttpContext http) =>
            {
                var userId = (Guid)http.Items["UserId"]!;
                var result = await mediator.Send(new GetCallParticipantQuery(id, pid, userId));
                return result is not null ? Results.Ok(result) : Results.NotFound();
            });
        }
    }
}