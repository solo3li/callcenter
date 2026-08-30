using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediatR;
using backend.Data;
using backend.Dtos;
using backend.Modules.Identity.Models;
using backend.Modules.Identity.Services;
using backend.Models.Enums;

namespace backend.Modules.Identity.Features.Auth.Login;

public record LoginCommand(string Email, string Password) : IRequest<AuthResponse>;

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponse>
{
    private readonly AppDbContext _db;
    private readonly TokenService _tokenService;

    public LoginCommandHandler(AppDbContext db, TokenService tokenService)
    {
        _db = db;
        _tokenService = tokenService;
    }

    public async Task<AuthResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower(), cancellationToken);
        if (user == null)
            throw new UnauthorizedAccessException("Invalid email or password");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password");

        if (user.Status != UserStatus.Active)
            throw new UnauthorizedAccessException("Account is not active");

        var accessToken = _tokenService.GenerateJwt(user);
        
        var dto = new UserDto(
            user.Id, user.Email, user.DisplayName, user.CompanyName,
            user.Status.ToString(), user.IsPartner,
            user.StandardCredits, user.PremiumCredits, user.CreatedAt);

        var expiresAt = DateTime.UtcNow.AddHours(24);
        var refreshToken = Guid.NewGuid().ToString("N");

        return new AuthResponse(accessToken, refreshToken, expiresAt, dto);
    }
}
