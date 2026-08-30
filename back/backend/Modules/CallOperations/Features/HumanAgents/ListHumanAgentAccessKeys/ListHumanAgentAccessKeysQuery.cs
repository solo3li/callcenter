using System;
using System.Collections.Generic;
using System.Linq;
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

namespace backend.Modules.CallOperations.Features.HumanAgents.ListHumanAgentAccessKeys;

public record ListHumanAgentAccessKeysQuery(Guid HumanAgentId, Guid OwnerUserId) : IRequest<List<AccessKeyListItem>>;

public class ListHumanAgentAccessKeysQueryHandler : IRequestHandler<ListHumanAgentAccessKeysQuery, List<AccessKeyListItem>>
{
    private readonly AppDbContext _db;

    public ListHumanAgentAccessKeysQueryHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<AccessKeyListItem>> Handle(ListHumanAgentAccessKeysQuery request, CancellationToken cancellationToken)
    {
        var agent = await _db.HumanAgents
            .FirstOrDefaultAsync(a => a.Id == request.HumanAgentId && a.OwnerUserId == request.OwnerUserId && a.IsActive, cancellationToken);

        if (agent is null)
            return new List<AccessKeyListItem>();

        return await _db.HumanAgentAccessKeys
            .Where(k => k.HumanAgentId == request.HumanAgentId)
            .OrderByDescending(k => k.CreatedAt)
            .Select(k => new AccessKeyListItem(
                k.Id,
                k.Name,
                k.KeyPrefix,
                k.Status,
                k.ExpiresAt,
                k.LastUsedAt,
                k.RevokedAt,
                k.CreatedAt
            ))
            .ToListAsync(cancellationToken);
    }
}
