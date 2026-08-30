using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using MediatR;
using backend.Modules.Identity.Dtos;
using backend.Modules.Billing.Dtos;
using backend.Modules.CallOperations.Dtos;
using backend.Modules.Configuration.Dtos;
using backend.Modules.Analytics.Dtos;
using backend.Modules.Identity.Features.ApiKeys.GetApiKeys;
using backend.Modules.Identity.Features.ApiKeys.CreateApiKey;
using backend.Modules.Identity.Features.ApiKeys.RevokeApiKey;
using backend.Modules.Identity.Features.ApiKeys.UpdateApiKeyScopes;

namespace backend.Modules.Identity.Endpoints
{
    public static class ApiKeyEndpoints
    {
        public static WebApplication MapApiKeyEndpoints(this WebApplication app)
        {
            app.MapGet("/api/api-keys", async (HttpContext context, IMediator mediator) =>
            {
                if (!context.Items.TryGetValue("UserId", out var userIdObj) || userIdObj is not Guid userId)
                    return Results.Unauthorized();

                var dtos = await mediator.Send(new GetApiKeysQuery(userId));
                return Results.Ok(dtos);
            });

            app.MapPost("/api/api-keys", async (CreateApiKeyRequest request, HttpContext context, IMediator mediator) =>
            {
                if (!context.Items.TryGetValue("UserId", out var userIdObj) || userIdObj is not Guid userId)
                    return Results.Unauthorized();

                var response = await mediator.Send(new CreateApiKeyCommand(userId, request));
                return Results.Ok(response);
            });

            app.MapDelete("/api/api-keys/{id:guid}", async (Guid id, HttpContext context, IMediator mediator) =>
            {
                if (!context.Items.TryGetValue("UserId", out var userIdObj) || userIdObj is not Guid userId)
                    return Results.Unauthorized();

                try
                {
                    await mediator.Send(new RevokeApiKeyCommand(id, userId));
                    return Results.Ok(new { message = "API key revoked" });
                }
                catch (InvalidOperationException)
                {
                    return Results.NotFound(new { error = "API key not found" });
                }
            });

            app.MapPatch("/api/api-keys/{id:guid}/scopes", async (Guid id, UpdateApiKeyScopesRequest request, HttpContext context, IMediator mediator) =>
            {
                if (!context.Items.TryGetValue("UserId", out var userIdObj) || userIdObj is not Guid userId)
                    return Results.Unauthorized();

                try
                {
                    await mediator.Send(new UpdateApiKeyScopesCommand(id, userId, request.Scopes));
                    return Results.Ok(new { message = "Scopes updated" });
                }
                catch (InvalidOperationException)
                {
                    return Results.NotFound(new { error = "API key not found" });
                }
            });

            return app;
        }
    }
}