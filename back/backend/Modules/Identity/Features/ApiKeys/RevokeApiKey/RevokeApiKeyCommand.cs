using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediatR;
using backend.Data;
using backend.Models.Enums;

namespace backend.Modules.Identity.Features.ApiKeys.RevokeApiKey;

public record RevokeApiKeyCommand(Guid KeyId, Guid UserId) : IRequest;

public class RevokeApiKeyCommandHandler : IRequestHandler<RevokeApiKeyCommand>
{
    private readonly AppDbContext _db;

    public RevokeApiKeyCommandHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task Handle(RevokeApiKeyCommand request, CancellationToken cancellationToken)
    {
        var key = await _db.ApiKeys.FirstOrDefaultAsync(k => k.Id == request.KeyId && k.UserId == request.UserId, cancellationToken);
        if (key == null)
            throw new InvalidOperationException("API key not found");

        key.Status = ApiKeyStatus.Revoked;
        key.RevokedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
    }
}
