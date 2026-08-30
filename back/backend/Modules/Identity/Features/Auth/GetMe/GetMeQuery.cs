using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediatR;
using backend.Data;
using backend.Dtos;
using backend.Modules.Identity.Models;
using backend.Modules.Identity.Services;

namespace backend.Modules.Identity.Features.Auth.GetMe;

public record GetMeQuery(Guid UserId) : IRequest<UserDto?>;

public class GetMeQueryHandler : IRequestHandler<GetMeQuery, UserDto?>
{
    private readonly AppDbContext _db;

    public GetMeQueryHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<UserDto?> Handle(GetMeQuery request, CancellationToken cancellationToken)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
        if (user == null)
            return null;

        return new UserDto(
            user.Id, user.Email, user.DisplayName, user.CompanyName,
            user.Status.ToString(), user.IsPartner,
            user.StandardCredits, user.PremiumCredits, user.CreatedAt);
    }
}
