using Microsoft.AspNetCore.Mvc;
using MediatR;
using backend.Dtos;
using backend.Modules.CallOperations.Features.HumanAgents.ListHumanAgents;
using backend.Modules.CallOperations.Features.HumanAgents.GetHumanAgent;
using backend.Modules.CallOperations.Features.HumanAgents.CreateHumanAgent;
using backend.Modules.CallOperations.Features.HumanAgents.UpdateHumanAgent;
using backend.Modules.CallOperations.Features.HumanAgents.DeleteHumanAgent;
using backend.Modules.CallOperations.Features.HumanAgents.UpdateHumanAgentStatus;
using backend.Modules.CallOperations.Features.HumanAgents.CreateHumanAgentAccessKey;
using backend.Modules.CallOperations.Features.HumanAgents.ListHumanAgentAccessKeys;
using backend.Modules.CallOperations.Features.HumanAgents.RevokeHumanAgentAccessKey;
using backend.Modules.CallOperations.Features.HumanAgents.ListHumanAgentSessions;
using backend.Modules.CallOperations.Features.HumanAgents.GetHumanAgentCurrentSession;

namespace backend.Endpoints;

public static class HumanAgentEndpoints
{
    public static void MapHumanAgentEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/human-agents");

        group.MapGet("/", async (IMediator mediator, HttpContext http) =>
        {
            var userId = (Guid)http.Items["UserId"]!;
            var agents = await mediator.Send(new ListHumanAgentsQuery(userId));
            return Results.Ok(agents);
        });

        group.MapGet("/{id:guid}", async (Guid id, IMediator mediator, HttpContext http) =>
        {
            var userId = (Guid)http.Items["UserId"]!;
            var agent = await mediator.Send(new GetHumanAgentQuery(id, userId));
            return agent is not null ? Results.Ok(agent) : Results.NotFound();
        });

        group.MapPost("/", async (CreateHumanAgentRequest request, IMediator mediator, HttpContext http) =>
        {
            var userId = (Guid)http.Items["UserId"]!;
            var agent = await mediator.Send(new CreateHumanAgentCommand(userId, request));
            return Results.Created($"/api/human-agents/{agent.Id}", agent);
        });

        group.MapPatch("/{id:guid}", async (Guid id, UpdateHumanAgentRequest request, IMediator mediator, HttpContext http) =>
        {
            var userId = (Guid)http.Items["UserId"]!;
            var agent = await mediator.Send(new UpdateHumanAgentCommand(id, userId, request));
            return agent is not null ? Results.Ok(agent) : Results.NotFound();
        });

        group.MapDelete("/{id:guid}", async (Guid id, IMediator mediator, HttpContext http) =>
        {
            var userId = (Guid)http.Items["UserId"]!;
            var deleted = await mediator.Send(new DeleteHumanAgentCommand(id, userId));
            return deleted ? Results.NoContent() : Results.NotFound();
        });

        group.MapPatch("/{id:guid}/status", async (Guid id, UpdateAgentStatusRequest request, IMediator mediator, HttpContext http) =>
        {
            var userId = (Guid)http.Items["UserId"]!;
            var agent = await mediator.Send(new UpdateHumanAgentStatusCommand(id, userId, request.Status));
            return agent is not null ? Results.Ok(agent) : Results.NotFound();
        });

        var keyGroup = group.MapGroup("/{humanAgentId:guid}/access-keys");

        keyGroup.MapGet("/", async (Guid humanAgentId, IMediator mediator, HttpContext http) =>
        {
            var userId = (Guid)http.Items["UserId"]!;
            var keys = await mediator.Send(new ListHumanAgentAccessKeysQuery(humanAgentId, userId));
            return Results.Ok(keys);
        });

        keyGroup.MapPost("/", async (Guid humanAgentId, CreateAccessKeyRequest request, IMediator mediator, HttpContext http) =>
        {
            var userId = (Guid)http.Items["UserId"]!;
            try
            {
                var response = await mediator.Send(new CreateHumanAgentAccessKeyCommand(humanAgentId, userId, request));
                return Results.Created($"/api/human-agents/{humanAgentId}/access-keys/{response.Id}", response);
            }
            catch (InvalidOperationException)
            {
                return Results.NotFound();
            }
        });

        keyGroup.MapDelete("/{keyId:guid}", async (Guid humanAgentId, Guid keyId, IMediator mediator, HttpContext http) =>
        {
            var userId = (Guid)http.Items["UserId"]!;
            var revoked = await mediator.Send(new RevokeHumanAgentAccessKeyCommand(humanAgentId, keyId, userId));
            return revoked ? Results.NoContent() : Results.NotFound();
        });

        var sessionGroup = group.MapGroup("/{humanAgentId:guid}/sessions");

        sessionGroup.MapGet("/", async (Guid humanAgentId, IMediator mediator, HttpContext http) =>
        {
            var userId = (Guid)http.Items["UserId"]!;
            var sessions = await mediator.Send(new ListHumanAgentSessionsQuery(humanAgentId, userId));
            return Results.Ok(sessions);
        });

        sessionGroup.MapGet("/current", async (Guid humanAgentId, IMediator mediator, HttpContext http) =>
        {
            var userId = (Guid)http.Items["UserId"]!;
            var session = await mediator.Send(new GetHumanAgentCurrentSessionQuery(humanAgentId, userId));
            return session is not null ? Results.Ok(session) : Results.NotFound();
        });
    }
}