using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediatR;
using backend.Data;
using backend.Modules.Identity.Dtos;
using backend.Modules.Billing.Dtos;
using backend.Modules.CallOperations.Dtos;
using backend.Modules.Configuration.Dtos;
using backend.Modules.Analytics.Dtos;
using backend.Modules.Identity.Models;
using backend.Modules.Identity.Services;
using backend.Models.Enums;

namespace backend.Modules.Identity.Features.Auth.Register;

public record RegisterCommand(string Email, string Password, string DisplayName, string? CompanyName) : IRequest<AuthResponse>;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResponse>
{
    private readonly AppDbContext _db;
    private readonly TokenService _tokenService;

    public RegisterCommandHandler(AppDbContext db, TokenService tokenService)
    {
        _db = db;
        _tokenService = tokenService;
    }

    public async Task<AuthResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var existing = await _db.Users.AnyAsync(u => u.Email.ToLower() == request.Email.ToLower(), cancellationToken);
        if (existing)
            throw new InvalidOperationException("Email already in use");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            DisplayName = request.DisplayName,
            CompanyName = request.CompanyName,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Status = UserStatus.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync(cancellationToken);

        var dto = new UserDto(
            user.Id, user.Email, user.DisplayName, user.CompanyName,
            user.Status.ToString(), user.IsPartner,
            user.StandardCredits, user.PremiumCredits, user.CreatedAt);

        var accessToken = _tokenService.GenerateJwt(user);
        var expiresAt = DateTime.UtcNow.AddHours(24);
        var refreshToken = Guid.NewGuid().ToString("N");

        return new AuthResponse(accessToken, refreshToken, expiresAt, dto);
    }
}
