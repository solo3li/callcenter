using backend.Dtos;
using backend.Services;

namespace backend.Endpoints;

public static class HumanAgentEndpoints
{
    public static void MapHumanAgentEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/human-agents");

        group.MapGet("/", async (HumanAgentService service, HttpContext http) =>
        {
            var userId = (Guid)http.Items["UserId"]!;
            var agents = await service.ListAsync(userId);
            return Results.Ok(agents);
        });

        group.MapGet("/{id:guid}", async (Guid id, HumanAgentService service, HttpContext http) =>
        {
            var userId = (Guid)http.Items["UserId"]!;
            var agent = await service.GetByIdAsync(id, userId);
            return agent is not null ? Results.Ok(agent) : Results.NotFound();
        });

        group.MapPost("/", async (CreateHumanAgentRequest request, HumanAgentService service, HttpContext http) =>
        {
            var userId = (Guid)http.Items["UserId"]!;
            var agent = await service.CreateAsync(userId, request);
            return Results.Created($"/api/human-agents/{agent.Id}", agent);
        });

        group.MapPatch("/{id:guid}", async (Guid id, UpdateHumanAgentRequest request, HumanAgentService service, HttpContext http) =>
        {
            var userId = (Guid)http.Items["UserId"]!;
            var agent = await service.UpdateAsync(id, userId, request);
            return agent is not null ? Results.Ok(agent) : Results.NotFound();
        });

        group.MapDelete("/{id:guid}", async (Guid id, HumanAgentService service, HttpContext http) =>
        {
            var userId = (Guid)http.Items["UserId"]!;
            var deleted = await service.DeleteAsync(id, userId);
            return deleted ? Results.NoContent() : Results.NotFound();
        });

        group.MapPatch("/{id:guid}/status", async (Guid id, UpdateAgentStatusRequest request, HumanAgentService service, HttpContext http) =>
        {
            var userId = (Guid)http.Items["UserId"]!;
            var agent = await service.UpdateStatusAsync(id, userId, request.Status);
            return agent is not null ? Results.Ok(agent) : Results.NotFound();
        });

        var keyGroup = group.MapGroup("/{humanAgentId:guid}/access-keys");

        keyGroup.MapGet("/", async (Guid humanAgentId, HumanAgentService service, HttpContext http) =>
        {
            var userId = (Guid)http.Items["UserId"]!;
            var keys = await service.ListAccessKeysAsync(humanAgentId, userId);
            return Results.Ok(keys);
        });

        keyGroup.MapPost("/", async (Guid humanAgentId, CreateAccessKeyRequest request, HumanAgentService service, HttpContext http) =>
        {
            var userId = (Guid)http.Items["UserId"]!;
            try
            {
                var response = await service.CreateAccessKeyAsync(humanAgentId, userId, request);
                return Results.Created($"/api/human-agents/{humanAgentId}/access-keys/{response.Id}", response);
            }
            catch (InvalidOperationException)
            {
                return Results.NotFound();
            }
        });

        keyGroup.MapDelete("/{keyId:guid}", async (Guid humanAgentId, Guid keyId, HumanAgentService service, HttpContext http) =>
        {
            var userId = (Guid)http.Items["UserId"]!;
            var revoked = await service.RevokeAccessKeyAsync(humanAgentId, keyId, userId);
            return revoked ? Results.NoContent() : Results.NotFound();
        });

        var sessionGroup = group.MapGroup("/{humanAgentId:guid}/sessions");

        sessionGroup.MapGet("/", async (Guid humanAgentId, HumanAgentService service, HttpContext http) =>
        {
            var userId = (Guid)http.Items["UserId"]!;
            var sessions = await service.ListSessionsAsync(humanAgentId, userId);
            return Results.Ok(sessions);
        });

        sessionGroup.MapGet("/current", async (Guid humanAgentId, HumanAgentService service, HttpContext http) =>
        {
            var userId = (Guid)http.Items["UserId"]!;
            var session = await service.GetCurrentSessionAsync(humanAgentId, userId);
            return session is not null ? Results.Ok(session) : Results.NotFound();
        });
    }
}