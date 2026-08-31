using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using backend.Services;
using backend.Modules.Identity.Services;

namespace backend.Middleware
{
    public static class AuthMiddlewareExtensions
    {
        public static IApplicationBuilder UseAuthMiddleware(this IApplicationBuilder app)
        {
            return app.UseMiddleware<AuthMiddleware>();
        }
    }

    public class AuthMiddleware
    {
        private readonly RequestDelegate _next;

        public AuthMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, TokenService tokenService, ApiKeyValidationService apiKeyService)
        {
            var path = context.Request.Path.Value?.ToLowerInvariant() ?? string.Empty;

            if (path.StartsWith("/api/auth/register") ||
                path.StartsWith("/api/auth/login") ||
                path.StartsWith("/api/auth/refresh") ||
                path.StartsWith("/api/auth/agent-login") ||
                path.StartsWith("/api/health") ||
                path.StartsWith("/api/version") ||
                path.StartsWith("/hubs/") ||
                path.StartsWith("/hangfire") ||
                path.StartsWith("/swagger") ||
                path.StartsWith("/scalar") ||
                path.StartsWith("/api/token") ||
                path.StartsWith("/api/livekit/") ||
                path.StartsWith("/api/webhooks/") ||
                path.StartsWith("/api/call/"))
            {
                await _next(context);
                return;
            }

            // Persona published contract: authenticated via service token inside
            // the endpoint (worker), or normal user JWT (owner preview).
            if (path.StartsWith("/api/personas/") && path.EndsWith("/published"))
            {
                await _next(context);
                return;
            }

            // Worker RAG tool: same service-token-or-owner model inside the endpoint.
            if (path.StartsWith("/api/personas/") && path.EndsWith("/knowledge-context"))
            {
                await _next(context);
                return;
            }

            // Worker metering: POST /api/usage authenticates inside the endpoint
            // via service token + callSessionId ownership resolution.
            if (path == "/api/usage" &&
                string.Equals(context.Request.Method, "POST", StringComparison.OrdinalIgnoreCase))
            {
                await _next(context);
                return;
            }

            string? authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                var token = authHeader["Bearer ".Length..].Trim();
                var userId = tokenService.ValidateToken(token);
                if (userId.HasValue)
                {
                    context.Items["UserId"] = userId.Value;
                    await _next(context);
                    return;
                }
            }

            string? apiKey = context.Request.Headers["X-API-Key"].FirstOrDefault();
            if (!string.IsNullOrEmpty(apiKey))
            {
                var userId = await apiKeyService.ValidateApiKeyAsync(apiKey);
                if (userId.HasValue)
                {
                    context.Items["UserId"] = userId.Value;
                    await _next(context);
                    return;
                }
            }

            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync("{\"error\":\"Unauthorized\"}");
        }
    }
}