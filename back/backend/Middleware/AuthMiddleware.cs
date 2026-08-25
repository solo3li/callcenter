using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using backend.Services;

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

        public async Task InvokeAsync(HttpContext context, AuthService authService, ApiKeyService apiKeyService)
        {
            var path = context.Request.Path.Value?.ToLowerInvariant() ?? string.Empty;

            if (path.StartsWith("/api/auth/register") ||
                path.StartsWith("/api/auth/login") ||
                path.StartsWith("/api/auth/agent-login") ||
                path.StartsWith("/api/health") ||
                path.StartsWith("/api/version") ||
                path.StartsWith("/hubs/") ||
                path.StartsWith("/hangfire") ||
                path.StartsWith("/swagger") ||
                path.StartsWith("/api/token") ||
                path.StartsWith("/api/livekit/") ||
                path.StartsWith("/api/call/"))
            {
                await _next(context);
                return;
            }

            string? authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                var token = authHeader["Bearer ".Length..].Trim();
                var userId = await authService.ValidateTokenAsync(token);
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