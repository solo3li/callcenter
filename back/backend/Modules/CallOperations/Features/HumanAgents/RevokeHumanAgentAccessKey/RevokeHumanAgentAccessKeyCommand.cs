using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediatR;
using backend.Data;
using backend.Models.Enums;

namespace backend.Modules.CallOperations.Features.HumanAgents.RevokeHumanAgentAccessKey;

public record RevokeHumanAgentAccessKeyCommand(Guid HumanAgentId, Guid KeyId, Guid OwnerUserId) : IRequest<bool>;

public class RevokeHumanAgentAccessKeyCommandHandler : IRequestHandler<RevokeHumanAgentAccessKeyCommand, bool>
{
    private readonly AppDbContext _db;

    public RevokeHumanAgentAccessKeyCommandHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<bool> Handle(RevokeHumanAgentAccessKeyCommand request, CancellationToken cancellationToken)
    {
        var agent = await _db.HumanAgents
            .FirstOrDefaultAsync(a => a.Id == request.HumanAgentId && a.OwnerUserId == request.OwnerUserId && a.IsActive, cancellationToken);

        if (agent is null)
            return false;

        var key = await _db.HumanAgentAccessKeys
            .FirstOrDefaultAsync(k => k.Id == request.KeyId && k.HumanAgentId == request.HumanAgentId && k.Status == AccessKeyStatus.Active, cancellationToken);

        if (key is null)
            return false;

        key.Status = AccessKeyStatus.Revoked;
        key.RevokedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);

        return true;
    }
}
