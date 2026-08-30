using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using MediatR;
using backend.Modules.Identity.Dtos;
using backend.Modules.Billing.Dtos;
using backend.Modules.CallOperations.Dtos;
using backend.Modules.Configuration.Dtos;
using backend.Modules.Analytics.Dtos;
using backend.Modules.Configuration.Features.Workflows.ListWorkflows;
using backend.Modules.Configuration.Features.Workflows.GetWorkflow;
using backend.Modules.Configuration.Features.Workflows.CreateWorkflow;
using backend.Modules.Configuration.Features.Workflows.UpdateWorkflow;
using backend.Modules.Configuration.Features.Workflows.DeleteWorkflow;
using backend.Modules.Configuration.Features.Workflows.ListWorkflowVersions;
using backend.Modules.Configuration.Features.Workflows.CreateWorkflowVersion;
using backend.Modules.Configuration.Features.Workflows.GetWorkflowVersion;
using backend.Modules.Configuration.Features.Workflows.PublishWorkflowVersion;

namespace backend.Modules.Configuration.Endpoints
{
    public static class WorkflowEndpoints
    {
        public static WebApplication MapWorkflowEndpoints(this WebApplication app)
        {
            app.MapGet("/api/workflows", async (HttpContext context, IMediator mediator) =>
            {
                if (!context.Items.TryGetValue("UserId", out var userIdObj) || userIdObj is not Guid userId)
                    return Results.Unauthorized();

                var workflows = await mediator.Send(new ListWorkflowsQuery(userId));
                return Results.Ok(workflows);
            });

            app.MapGet("/api/workflows/{id:guid}", async (Guid id, IMediator mediator) =>
            {
                var workflow = await mediator.Send(new GetWorkflowQuery(id));
                return workflow == null ? Results.NotFound() : Results.Ok(workflow);
            });

            app.MapPost("/api/workflows", async (HttpContext context, CreateWorkflowRequest request, IMediator mediator) =>
            {
                if (!context.Items.TryGetValue("UserId", out var userIdObj) || userIdObj is not Guid userId)
                    return Results.Unauthorized();

                var workflow = await mediator.Send(new CreateWorkflowCommand(userId, request.Name, request.Description));
                return Results.Created($"/api/workflows/{workflow.Id}", workflow);
            });

            app.MapPut("/api/workflows/{id:guid}", async (Guid id, UpdateWorkflowRequest request, IMediator mediator) =>
            {
                var workflow = await mediator.Send(new UpdateWorkflowCommand(id, request.Name, request.Description, request.IsActive));
                return workflow == null ? Results.NotFound() : Results.Ok(workflow);
            });

            app.MapDelete("/api/workflows/{id:guid}", async (Guid id, IMediator mediator) =>
            {
                var deleted = await mediator.Send(new DeleteWorkflowCommand(id));
                return deleted ? Results.Ok(new { message = "Workflow deleted" }) : Results.NotFound();
            });

            app.MapGet("/api/workflows/{workflowId:guid}/versions", async (Guid workflowId, IMediator mediator) =>
            {
                var versions = await mediator.Send(new ListWorkflowVersionsQuery(workflowId));
                return Results.Ok(versions);
            });

            app.MapPost("/api/workflows/{workflowId:guid}/versions", async (Guid workflowId, CreateWorkflowVersionRequest request, IMediator mediator) =>
            {
                var version = await mediator.Send(new CreateWorkflowVersionCommand(workflowId, request.DefinitionJson));
                return Results.Created($"/api/workflow-versions/{version.Id}", version);
            });

            app.MapGet("/api/workflow-versions/{versionId:guid}", async (Guid versionId, IMediator mediator) =>
            {
                var version = await mediator.Send(new GetWorkflowVersionQuery(versionId));
                return version == null ? Results.NotFound() : Results.Ok(version);
            });

            app.MapPost("/api/workflow-versions/{versionId:guid}/publish", async (Guid versionId, IMediator mediator) =>
            {
                var version = await mediator.Send(new PublishWorkflowVersionCommand(versionId));
                return version == null ? Results.NotFound() : Results.Ok(version);
            });

            return app;
        }
    }
}