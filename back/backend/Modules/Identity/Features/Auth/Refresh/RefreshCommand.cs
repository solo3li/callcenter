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

namespace backend.Modules.Identity.Features.Auth.Refresh;

public record RefreshCommand(string Token) : IRequest<AuthResponse?>;

public class RefreshCommandHandler : IRequestHandler<RefreshCommand, AuthResponse?>
{
    private readonly AppDbContext _db;
    private readonly TokenService _tokenService;

    public RefreshCommandHandler(AppDbContext db, TokenService tokenService)
    {
        _db = db;
        _tokenService = tokenService;
    }

    public async Task<AuthResponse?> Handle(RefreshCommand request, CancellationToken cancellationToken)
    {
        var userId = _tokenService.ValidateToken(request.Token);
        if (!userId.HasValue)
            return null;

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId.Value, cancellationToken);
        if (user == null)
            return null;

        var newToken = _tokenService.GenerateJwt(user);
        var expiresAt = DateTime.UtcNow.AddHours(24);
        var refreshToken = Guid.NewGuid().ToString("N");

        var dto = new UserDto(
            user.Id, user.Email, user.DisplayName, user.CompanyName,
            user.Status.ToString(), user.IsPartner,
            user.StandardCredits, user.PremiumCredits, user.CreatedAt);

        return new AuthResponse(newToken, refreshToken, expiresAt, dto);
    }
}
