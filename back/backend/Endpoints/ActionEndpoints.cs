using backend.Data;
using backend.Dtos;
using backend.Models.Domain;
using backend.Models.Enums;
using backend.Services;
using Microsoft.EntityFrameworkCore;

namespace backend.Endpoints;

public static class ActionEndpoints
{
    public static void MapActionEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/actions");

        group.MapGet("/", async (string? type, ActionService service) =>
        {
            var actions = await service.ListAsync(type);
            return Results.Ok(actions);
        });

        group.MapGet("/system", async (ActionService service) =>
        {
            var actions = await service.ListSystemAsync();
            return Results.Ok(actions);
        });

        group.MapGet("/{id:guid}", async (Guid id, ActionService service) =>
        {
            var action = await service.GetByIdAsync(id);
            return action is not null ? Results.Ok(action) : Results.NotFound();
        });

        group.MapPost("/", async (CreateActionRequest request, ActionService service) =>
        {
            if (request.ActionType != ActionType.Integration && request.ActionType != ActionType.Webhook)
                return Results.BadRequest("Users can only create Integration or Webhook action definitions.");

            var action = await service.CreateAsync(request);
            return Results.Created($"/api/actions/{action.Id}", action);
        });

        group.MapPatch("/{id:guid}", async (Guid id, UpdateActionRequest request, ActionService service) =>
        {
            try
            {
                var action = await service.UpdateAsync(id, request);
                return action is not null ? Results.Ok(action) : Results.NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(ex.Message);
            }
        });

        group.MapDelete("/{id:guid}", async (Guid id, ActionService service) =>
        {
            try
            {
                var deleted = await service.DeleteAsync(id);
                return deleted ? Results.NoContent() : Results.NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(ex.Message);
            }
        });

        var execGroup = group.MapGroup("/executions");

        execGroup.MapGet("/{id:guid}", async (Guid id, AppDbContext db) =>
        {
            var execution = await db.ActionExecutions
                .Include(e => e.ActionDefinition)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (execution is null)
                return Results.NotFound();

            return Results.Ok(new
            {
                execution.Id,
                execution.CallSessionId,
                execution.ActionDefinitionId,
                ActionDefinitionName = execution.ActionDefinition.DisplayName,
                execution.WorkflowExecutionId,
                Status = execution.Status.ToString(),
                execution.InputJson,
                execution.OutputJson,
                execution.Error,
                execution.StartedAt,
                execution.CompletedAt
            });
        });

        execGroup.MapGet("/by-call/{callSessionId:guid}", async (Guid callSessionId, AppDbContext db) =>
        {
            var executions = await db.ActionExecutions
                .Where(e => e.CallSessionId == callSessionId)
                .Include(e => e.ActionDefinition)
                .OrderByDescending(e => e.StartedAt)
                .Select(e => new
                {
                    e.Id,
                    e.CallSessionId,
                    e.ActionDefinitionId,
                    ActionDefinitionName = e.ActionDefinition.DisplayName,
                    e.WorkflowExecutionId,
                    Status = e.Status.ToString(),
                    e.InputJson,
                    e.OutputJson,
                    e.Error,
                    e.StartedAt,
                    e.CompletedAt
                })
                .ToListAsync();

            return Results.Ok(executions);
        });
    }
}