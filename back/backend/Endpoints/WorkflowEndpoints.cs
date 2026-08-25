using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using backend.Dtos;
using backend.Services;

namespace backend.Endpoints
{
    public static class WorkflowEndpoints
    {
        public static WebApplication MapWorkflowEndpoints(this WebApplication app)
        {
            app.MapGet("/api/workflows", async (HttpContext context, WorkflowService service) =>
            {
                if (!context.Items.TryGetValue("UserId", out var userIdObj) || userIdObj is not Guid userId)
                    return Results.Unauthorized();

                var workflows = await service.ListAsync(userId);
                return Results.Ok(workflows);
            });

            app.MapGet("/api/workflows/{id:guid}", async (Guid id, WorkflowService service) =>
            {
                var workflow = await service.GetByIdAsync(id);
                return workflow == null ? Results.NotFound() : Results.Ok(workflow);
            });

            app.MapPost("/api/workflows", async (HttpContext context,
                CreateWorkflowRequest request, WorkflowService service) =>
            {
                if (!context.Items.TryGetValue("UserId", out var userIdObj) || userIdObj is not Guid userId)
                    return Results.Unauthorized();

                var workflow = await service.CreateAsync(userId, request);
                return Results.Created($"/api/workflows/{workflow.Id}", workflow);
            });

            app.MapPut("/api/workflows/{id:guid}", async (Guid id,
                UpdateWorkflowRequest request, WorkflowService service) =>
            {
                var workflow = await service.UpdateAsync(id, request);
                return workflow == null ? Results.NotFound() : Results.Ok(workflow);
            });

            app.MapDelete("/api/workflows/{id:guid}", async (Guid id, WorkflowService service) =>
            {
                var deleted = await service.DeleteAsync(id);
                return deleted ? Results.Ok(new { message = "Workflow deleted" }) : Results.NotFound();
            });

            app.MapGet("/api/workflows/{workflowId:guid}/versions", async (Guid workflowId,
                WorkflowService service) =>
            {
                var versions = await service.ListVersionsAsync(workflowId);
                return Results.Ok(versions);
            });

            app.MapPost("/api/workflows/{workflowId:guid}/versions", async (Guid workflowId,
                CreateWorkflowVersionRequest request, WorkflowService service) =>
            {
                var version = await service.CreateVersionAsync(workflowId, request);
                return Results.Created($"/api/workflow-versions/{version.Id}", version);
            });

            app.MapGet("/api/workflow-versions/{versionId:guid}", async (Guid versionId,
                WorkflowService service) =>
            {
                var version = await service.GetVersionAsync(versionId);
                return version == null ? Results.NotFound() : Results.Ok(version);
            });

            app.MapPost("/api/workflow-versions/{versionId:guid}/publish", async (Guid versionId,
                WorkflowService service) =>
            {
                var version = await service.PublishVersionAsync(versionId);
                return version == null ? Results.NotFound() : Results.Ok(version);
            });

            return app;
        }
    }
}