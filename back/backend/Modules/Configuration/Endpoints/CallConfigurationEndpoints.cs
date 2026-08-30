using Microsoft.AspNetCore.Mvc;
using MediatR;
using backend.Modules.Identity.Dtos;
using backend.Modules.Billing.Dtos;
using backend.Modules.CallOperations.Dtos;
using backend.Modules.Configuration.Dtos;
using backend.Modules.Analytics.Dtos;
using backend.Modules.Configuration.Features.CallConfigurations.ListCallConfigurations;
using backend.Modules.Configuration.Features.CallConfigurations.GetCallConfiguration;
using backend.Modules.Configuration.Features.CallConfigurations.CreateCallConfiguration;
using backend.Modules.Configuration.Features.CallConfigurations.UpdateCallConfiguration;
using backend.Modules.Configuration.Features.CallConfigurations.DeleteCallConfiguration;
using backend.Modules.Configuration.Features.CallConfigurations.ActivateCallConfiguration;
using backend.Modules.Configuration.Features.CallConfigurations.SetCallConfigurationActions;
using backend.Modules.Configuration.Features.CallConfigurations.GetCallConfigurationActions;

namespace backend.Modules.Configuration.Endpoints
{
    public static class CallConfigurationEndpoints
    {
        public static void MapCallConfigurationEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/call-configurations");

            group.MapGet("/", async (
                IMediator mediator,
                HttpContext http) =>
            {
                var userId = (Guid)http.Items["UserId"]!;
                var result = await mediator.Send(new ListCallConfigurationsQuery(userId));
                return Results.Ok(result);
            });

            group.MapGet("/{id:guid}", async (
                Guid id,
                IMediator mediator,
                HttpContext http) =>
            {
                var userId = (Guid)http.Items["UserId"]!;
                var result = await mediator.Send(new GetCallConfigurationQuery(id, userId));
                return result is not null ? Results.Ok(result) : Results.NotFound();
            });

            group.MapPost("/", async (
                [FromBody] CreateCallConfigRequest request,
                IMediator mediator,
                HttpContext http) =>
            {
                var userId = (Guid)http.Items["UserId"]!;
                var result = await mediator.Send(new CreateCallConfigurationCommand(userId, request));
                return Results.Created($"/api/call-configurations/{result.Id}", result);
            });

            group.MapPatch("/{id:guid}", async (
                Guid id,
                [FromBody] UpdateCallConfigRequest request,
                IMediator mediator,
                HttpContext http) =>
            {
                var userId = (Guid)http.Items["UserId"]!;
                var result = await mediator.Send(new UpdateCallConfigurationCommand(id, userId, request));
                return result is not null ? Results.Ok(result) : Results.NotFound();
            });

            group.MapDelete("/{id:guid}", async (
                Guid id,
                IMediator mediator,
                HttpContext http) =>
            {
                var userId = (Guid)http.Items["UserId"]!;
                var deleted = await mediator.Send(new DeleteCallConfigurationCommand(id, userId));
                return deleted ? Results.NoContent() : Results.NotFound();
            });

            group.MapPost("/{id:guid}/activate", async (
                Guid id,
                IMediator mediator,
                HttpContext http) =>
            {
                var userId = (Guid)http.Items["UserId"]!;
                var result = await mediator.Send(new ActivateCallConfigurationCommand(id, userId));
                return result is not null ? Results.Ok(result) : Results.NotFound();
            });

            group.MapPut("/{id:guid}/actions", async (
                Guid id,
                [FromBody] SetConfigActionsRequest request,
                IMediator mediator,
                HttpContext http) =>
            {
                var userId = (Guid)http.Items["UserId"]!;
                var result = await mediator.Send(new SetCallConfigurationActionsCommand(id, userId, request));
                return result is not null ? Results.Ok(result) : Results.NotFound();
            });

            group.MapGet("/{id:guid}/actions", async (
                Guid id,
                IMediator mediator,
                HttpContext http) =>
            {
                var userId = (Guid)http.Items["UserId"]!;
                var result = await mediator.Send(new GetCallConfigurationActionsQuery(id, userId));
                return Results.Ok(result);
            });
        }
    }
}