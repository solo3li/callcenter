using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using FluentValidation;
using MediatR;
using backend.Modules.Identity.Dtos;
using backend.Modules.Billing.Dtos;
using backend.Modules.CallOperations.Dtos;
using backend.Modules.Configuration.Dtos;
using backend.Modules.Analytics.Dtos;
using backend.Modules.Identity.Features.Auth.Login;
using backend.Modules.Identity.Features.Auth.Register;
using backend.Modules.Identity.Features.Auth.Refresh;
using backend.Modules.Identity.Features.Auth.GetMe;
using backend.Modules.Identity.Features.Auth.AgentLogin;

namespace backend.Modules.Identity.Endpoints
{
    public static class AuthEndpoints
    {
        public static WebApplication MapAuthEndpoints(this WebApplication app)
        {
            app.MapPost("/api/auth/register", async (RegisterRequest request, IMediator mediator, IValidator<RegisterRequest> validator) =>
            {
                var validationResult = await validator.ValidateAsync(request);
                if (!validationResult.IsValid)
                    return Results.ValidationProblem(validationResult.ToDictionary());

                try
                {
                    var response = await mediator.Send(new RegisterCommand(request.Email, request.Password, request.DisplayName, request.CompanyName));
                    return Results.Ok(response);
                }
                catch (InvalidOperationException ex)
                {
                    return Results.Conflict(new { error = ex.Message });
                }
            }).RequireRateLimiting("auth");

            app.MapPost("/api/auth/login", async (LoginRequest request, IMediator mediator, IValidator<LoginRequest> validator) =>
            {
                var validationResult = await validator.ValidateAsync(request);
                if (!validationResult.IsValid)
                    return Results.ValidationProblem(validationResult.ToDictionary());

                try
                {
                    var response = await mediator.Send(new LoginCommand(request.Email, request.Password));
                    return Results.Ok(response);
                }
                catch (UnauthorizedAccessException)
                {
                    return Results.Unauthorized();
                }
            }).RequireRateLimiting("auth");

            app.MapPost("/api/auth/refresh", async (HttpContext context, IMediator mediator) =>
            {
                var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
                if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                    return Results.Unauthorized();

                var token = authHeader["Bearer ".Length..].Trim();
                
                var response = await mediator.Send(new RefreshCommand(token));
                if (response == null)
                    return Results.Unauthorized();

                return Results.Ok(response);
            });

            app.MapPost("/api/auth/logout", () => Results.Ok(new { message = "Logged out" }));

            app.MapGet("/api/auth/me", async (HttpContext context, IMediator mediator) =>
            {
                if (!context.Items.TryGetValue("UserId", out var userIdObj) || userIdObj is not Guid userId)
                    return Results.Unauthorized();

                var response = await mediator.Send(new GetMeQuery(userId));
                if (response == null)
                    return Results.NotFound();

                return Results.Ok(response);
            });

            app.MapPost("/api/auth/agent-login", async (AgentLoginRequest request, IMediator mediator) =>
            {
                var response = await mediator.Send(new AgentLoginCommand(request.AccessKey));
                if (response == null)
                    return Results.Unauthorized();

                return Results.Ok(response);
            });

            return app;
        }
    }
}