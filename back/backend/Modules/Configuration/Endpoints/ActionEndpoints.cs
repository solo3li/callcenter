using backend.Modules.Identity.Dtos;
using backend.Modules.Billing.Dtos;
using backend.Modules.CallOperations.Dtos;
using backend.Modules.Configuration.Dtos;
using backend.Modules.Analytics.Dtos;
using backend.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using backend.Modules.Configuration.Features.Actions.ListActions;
using backend.Modules.Configuration.Features.Actions.ListSystemActions;
using backend.Modules.Configuration.Features.Actions.GetAction;
using backend.Modules.Configuration.Features.Actions.CreateAction;
using backend.Modules.Configuration.Features.Actions.UpdateAction;
using backend.Modules.Configuration.Features.Actions.DeleteAction;
using backend.Modules.Configuration.Features.Actions.GetActionExecution;
using backend.Modules.Configuration.Features.Actions.ListActionExecutionsByCall;

namespace backend.Modules.Configuration.Endpoints;

public static class ActionEndpoints
{
    public static void MapActionEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/actions");

        group.MapGet("/", async (string? type, IMediator mediator) =>
        {
            var actions = await mediator.Send(new ListActionsQuery(type));
            return Results.Ok(actions);
        });

        group.MapGet("/system", async (IMediator mediator) =>
        {
            var actions = await mediator.Send(new ListSystemActionsQuery());
            return Results.Ok(actions);
        });

        group.MapGet("/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var action = await mediator.Send(new GetActionQuery(id));
            return action is not null ? Results.Ok(action) : Results.NotFound();
        });

        group.MapPost("/", async (CreateActionRequest request, IMediator mediator) =>
        {
            if (request.ActionType != ActionType.Integration && request.ActionType != ActionType.Webhook)
                return Results.BadRequest("Users can only create Integration or Webhook action definitions.");

            var action = await mediator.Send(new CreateActionCommand(request));
            return Results.Created($"/api/actions/{action.Id}", action);
        });

        group.MapPatch("/{id:guid}", async (Guid id, UpdateActionRequest request, IMediator mediator) =>
        {
            try
            {
                var action = await mediator.Send(new UpdateActionCommand(id, request));
                return action is not null ? Results.Ok(action) : Results.NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(ex.Message);
            }
        });

        group.MapDelete("/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            try
            {
                var deleted = await mediator.Send(new DeleteActionCommand(id));
                return deleted ? Results.NoContent() : Results.NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(ex.Message);
            }
        });

        var execGroup = group.MapGroup("/executions");

        execGroup.MapGet("/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var execution = await mediator.Send(new GetActionExecutionQuery(id));
            if (execution is null)
                return Results.NotFound();

            return Results.Ok(execution);
        });

        execGroup.MapGet("/by-call/{callSessionId:guid}", async (Guid callSessionId, IMediator mediator) =>
        {
            var executions = await mediator.Send(new ListActionExecutionsByCallQuery(callSessionId));
            return Results.Ok(executions);
        });
    }
}