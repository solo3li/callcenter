using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using backend.Dtos;
using backend.Services;

namespace backend.Endpoints
{
    public static class ApiKeyEndpoints
    {
        public static WebApplication MapApiKeyEndpoints(this WebApplication app)
        {
            app.MapGet("/api/api-keys", async (HttpContext context, ApiKeyService apiKeyService) =>
            {
                if (!context.Items.TryGetValue("UserId", out var userIdObj) || userIdObj is not Guid userId)
                    return Results.Unauthorized();

                var keys = await apiKeyService.ListAsync(userId);
                var dtos = keys.Select(k => new ApiKeyListItem(
                    k.Id,
                    k.Name,
                    k.KeyPrefix,
                    k.Status.ToString(),
                    k.Scopes,
                    k.LastUsedAt,
                    k.ExpiresAt,
                    k.CreatedAt
                ));

                return Results.Ok(dtos);
            });

            app.MapPost("/api/api-keys", async (CreateApiKeyRequest request, HttpContext context, ApiKeyService apiKeyService) =>
            {
                if (!context.Items.TryGetValue("UserId", out var userIdObj) || userIdObj is not Guid userId)
                    return Results.Unauthorized();

                var (apiKey, rawKey) = await apiKeyService.CreateAsync(userId, request);
                var response = new CreateApiKeyResponse(apiKey.Id, apiKey.Name, rawKey, apiKey.KeyPrefix, apiKey.CreatedAt);
                return Results.Ok(response);
            });

            app.MapDelete("/api/api-keys/{id:guid}", async (Guid id, HttpContext context, ApiKeyService apiKeyService) =>
            {
                if (!context.Items.TryGetValue("UserId", out var userIdObj) || userIdObj is not Guid userId)
                    return Results.Unauthorized();

                try
                {
                    await apiKeyService.RevokeAsync(id, userId);
                    return Results.Ok(new { message = "API key revoked" });
                }
                catch (InvalidOperationException)
                {
                    return Results.NotFound(new { error = "API key not found" });
                }
            });

            app.MapPatch("/api/api-keys/{id:guid}/scopes", async (Guid id, UpdateApiKeyScopesRequest request, HttpContext context, ApiKeyService apiKeyService) =>
            {
                if (!context.Items.TryGetValue("UserId", out var userIdObj) || userIdObj is not Guid userId)
                    return Results.Unauthorized();

                try
                {
                    await apiKeyService.UpdateScopesAsync(id, userId, request.Scopes);
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