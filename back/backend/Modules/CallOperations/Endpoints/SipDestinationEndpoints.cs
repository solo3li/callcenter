using System.Text.Json;
using MediatR;
using backend.Data;
using backend.Middleware;
using backend.Models.Domain;
using backend.Models.Enums;
using backend.Modules.CallOperations.Features.SipDestinations.ListSipDestinations;
using backend.Modules.CallOperations.Features.SipDestinations.CreateSipDestination;
using backend.Modules.CallOperations.Features.SipDestinations.UpdateSipDestination;
using backend.Modules.CallOperations.Features.SipDestinations.DeleteSipDestination;
using backend.Modules.CallOperations.Features.SipDestinations.GetTransferOptions;
using Microsoft.EntityFrameworkCore;

namespace backend.Modules.CallOperations.Endpoints;

public static class SipDestinationEndpoints
{
    public static WebApplication MapSipDestinationEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/sip/destinations");

        group.MapGet("/", async (IMediator mediator, HttpContext http) =>
        {
            var userId = (Guid)http.Items["UserId"]!;
            var items = await mediator.Send(new ListSipDestinationsQuery(userId));
            return Results.Ok(items);
        });

        group.MapPost("/", async (CreateSipDestinationRequest req, IMediator mediator, HttpContext http) =>
        {
            var userId = (Guid)http.Items["UserId"]!;
            try 
            {
                var destination = await mediator.Send(new CreateSipDestinationCommand(userId, req));
                return Results.Created($"/api/sip/destinations/{destination.Id}", destination);
            }
            catch (InvalidOperationException ex)
            {
                if (ex.Message.Contains("already exists"))
                    return Results.Conflict(new { error = ex.Message });
                return Results.BadRequest(new { error = ex.Message });
            }
        });

        group.MapPatch("/{id:guid}", async (Guid id, UpdateSipDestinationRequest req,
            IMediator mediator, HttpContext http) =>
        {
            var userId = (Guid)http.Items["UserId"]!;
            try
            {
                var destination = await mediator.Send(new UpdateSipDestinationCommand(id, userId, req));
                if (destination == null) return Results.NotFound();
                return Results.Ok(destination);
            }
            catch (InvalidOperationException ex)
            {
                return Results.Conflict(new { error = ex.Message });
            }
        });

        group.MapDelete("/{id:guid}", async (Guid id, IMediator mediator, HttpContext http) =>
        {
            var userId = (Guid)http.Items["UserId"]!;
            var deleted = await mediator.Send(new DeleteSipDestinationCommand(id, userId));
            if (!deleted) return Results.NotFound();
            return Results.NoContent();
        });

        // ── transfer options for the AI layer (names only, never CallTo) ──
        group.MapGet("/options", async (IMediator mediator, HttpContext http) =>
        {
            var userId = (Guid)http.Items["UserId"]!;
            var result = await mediator.Send(new GetTransferOptionsQuery(userId));
            return Results.Ok(new { agents = result.Agents, destinations = result.Destinations });
        });

        return app;
    }
}

