using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Security.Claims;
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

namespace backend.Modules.Identity.Features.Auth.Refresh;

public record RefreshCommand(string AccessToken, string RefreshToken) : IRequest<AuthResponse?>;

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
        var principal = _tokenService.GetPrincipalFromExpiredToken(request.AccessToken);
        if (principal == null)
            return null;

        var sub = principal.Claims.FirstOrDefault(c => c.Type == "sub" || c.Type == ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(sub) || !Guid.TryParse(sub, out var userId))
            return null;

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user == null || user.RefreshToken != request.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            return null;

        var newToken = _tokenService.GenerateJwt(user);
        var expiresAt = DateTime.UtcNow.AddHours(24);
        var newRefreshToken = Guid.NewGuid().ToString("N");

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await _db.SaveChangesAsync(cancellationToken);

        var dto = new UserDto(
            user.Id, user.Email, user.DisplayName, user.CompanyName,
            user.Status.ToString(), user.IsPartner,
            user.StandardCredits, user.PremiumCredits, user.CreatedAt);

        return new AuthResponse(newToken, newRefreshToken, expiresAt, dto);
    }
}
