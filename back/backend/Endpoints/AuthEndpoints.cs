using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using Jose;
using backend.Data;
using backend.Dtos;
using backend.Models.Domain;
using backend.Models.Enums;
using backend.Services;

namespace backend.Endpoints
{
    public static class AuthEndpoints
    {
        public static WebApplication MapAuthEndpoints(this WebApplication app)
        {
            app.MapPost("/api/auth/register", async (RegisterRequest request, AuthService authService) =>
            {
                var validator = app.Services.GetRequiredService<IValidator<RegisterRequest>>();
                var validationResult = await validator.ValidateAsync(request);
                if (!validationResult.IsValid)
                    return Results.ValidationProblem(validationResult.ToDictionary());

                try
                {
                    var user = await authService.RegisterAsync(request);
                    var dto = new UserDto(
                        user.Id, user.Email, user.DisplayName, user.CompanyName,
                        user.Status.ToString(), user.IsPartner,
                        user.StandardCredits, user.PremiumCredits, user.CreatedAt);

                    var accessToken = authService.GenerateJwt(user);
                    var expiresAt = DateTime.UtcNow.AddHours(24);
                    var refreshToken = Guid.NewGuid().ToString("N");

                    return Results.Ok(new AuthResponse(accessToken, refreshToken, expiresAt, dto));
                }
                catch (InvalidOperationException ex)
                {
                    return Results.Conflict(new { error = ex.Message });
                }
            }).RequireRateLimiting("auth");

            app.MapPost("/api/auth/login", async (LoginRequest request, AuthService authService) =>
            {
                var validator = app.Services.GetRequiredService<IValidator<LoginRequest>>();
                var validationResult = await validator.ValidateAsync(request);
                if (!validationResult.IsValid)
                    return Results.ValidationProblem(validationResult.ToDictionary());

                try
                {
                    var accessToken = await authService.LoginAsync(request);

                    var db = app.Services.GetRequiredService<AppDbContext>();
                    var user = await db.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower());
                    if (user == null)
                        return Results.Unauthorized();

                    var dto = new UserDto(
                        user.Id, user.Email, user.DisplayName, user.CompanyName,
                        user.Status.ToString(), user.IsPartner,
                        user.StandardCredits, user.PremiumCredits, user.CreatedAt);

                    var expiresAt = DateTime.UtcNow.AddHours(24);
                    var refreshToken = Guid.NewGuid().ToString("N");

                    return Results.Ok(new AuthResponse(accessToken, refreshToken, expiresAt, dto));
                }
                catch (UnauthorizedAccessException)
                {
                    return Results.Unauthorized();
                }
            }).RequireRateLimiting("auth");

            app.MapPost("/api/auth/refresh", async (HttpContext context, AuthService authService) =>
            {
                var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
                if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                    return Results.Unauthorized();

                var token = authHeader["Bearer ".Length..].Trim();
                var userId = await authService.ValidateTokenAsync(token);
                if (!userId.HasValue)
                    return Results.Unauthorized();

                var user = await authService.GetUserByIdAsync(userId.Value);
                if (user == null)
                    return Results.Unauthorized();

                var newToken = authService.GenerateJwt(user);
                var expiresAt = DateTime.UtcNow.AddHours(24);
                var refreshToken = Guid.NewGuid().ToString("N");

                var dto = new UserDto(
                    user.Id, user.Email, user.DisplayName, user.CompanyName,
                    user.Status.ToString(), user.IsPartner,
                    user.StandardCredits, user.PremiumCredits, user.CreatedAt);

                return Results.Ok(new AuthResponse(newToken, refreshToken, expiresAt, dto));
            });

            app.MapPost("/api/auth/logout", () => Results.Ok(new { message = "Logged out" }));

            app.MapGet("/api/auth/me", async (HttpContext context, AuthService authService) =>
            {
                if (!context.Items.TryGetValue("UserId", out var userIdObj) || userIdObj is not Guid userId)
                    return Results.Unauthorized();

                var user = await authService.GetUserByIdAsync(userId);
                if (user == null)
                    return Results.NotFound();

                var dto = new UserDto(
                    user.Id, user.Email, user.DisplayName, user.CompanyName,
                    user.Status.ToString(), user.IsPartner,
                    user.StandardCredits, user.PremiumCredits, user.CreatedAt);

                return Results.Ok(dto);
            });

            app.MapPost("/api/auth/agent-login", async (AgentLoginRequest request, AppDbContext db) =>
            {
                var accessKey = await db.HumanAgentAccessKeys
                    .Include(k => k.HumanAgent)
                    .FirstOrDefaultAsync(k => request.AccessKey == k.KeyHash);

                if (accessKey == null)
                {
                    var keys = await db.HumanAgentAccessKeys.ToListAsync();
                    HumanAgentAccessKey? matched = null;
                    foreach (var k in keys)
                    {
                        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(request.AccessKey));
                        var hashHex = Convert.ToHexString(hash).ToLowerInvariant();
                        if (k.KeyHash == hashHex)
                        {
                            matched = k;
                            break;
                        }
                    }
                    if (matched == null)
                        return Results.Unauthorized();
                    await db.Entry(matched).Reference(k => k.HumanAgent).LoadAsync();
                    accessKey = matched;
                }

                if (accessKey.Status != AccessKeyStatus.Active)
                    return Results.Unauthorized();

                if (accessKey.ExpiresAt.HasValue && accessKey.ExpiresAt.Value < DateTime.UtcNow)
                    return Results.Unauthorized();

                accessKey.LastUsedAt = DateTime.UtcNow;
                await db.SaveChangesAsync();

                var agent = accessKey.HumanAgent;
                var apiKey = Environment.GetEnvironmentVariable("LIVEKIT_API_KEY") ?? "devkey";
                var apiSecret = Environment.GetEnvironmentVariable("LIVEKIT_API_SECRET") ?? "secret";
                var livekitUrl = Environment.GetEnvironmentVariable("LIVEKIT_URL") ?? "ws://127.0.0.1:7880";

                var identity = $"agent_{agent.Id.ToString("N")[..8]}";
                var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                var payload = new Dictionary<string, object>
                {
                    { "iss", apiKey },
                    { "sub", identity },
                    { "name", agent.Name },
                    { "nbf", now },
                    { "exp", now + 7200 },
                    { "video", new Dictionary<string, object>
                        {
                            { "roomJoin", true },
                            { "room", "*" },
                            { "canPublish", true },
                            { "canSubscribe", true }
                        }
                    }
                };

                var livekitToken = JWT.Encode(payload, Encoding.UTF8.GetBytes(apiSecret), JwsAlgorithm.HS256);

                return Results.Ok(new AgentLoginResponse(
                    agent.Id,
                    agent.Name,
                    livekitToken,
                    livekitUrl,
                    agent.OwnerUserId.ToString()
                ));
            });

            return app;
        }
    }
}