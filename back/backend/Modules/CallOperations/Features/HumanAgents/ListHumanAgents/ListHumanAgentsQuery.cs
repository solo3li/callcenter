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

namespace backend.Modules.CallOperations.Features.HumanAgents.ListHumanAgents;

public record ListHumanAgentsQuery(Guid OwnerUserId) : IRequest<List<HumanAgentListItem>>;

public class ListHumanAgentsQueryHandler : IRequestHandler<ListHumanAgentsQuery, List<HumanAgentListItem>>
{
    private readonly AppDbContext _db;

    public ListHumanAgentsQueryHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<HumanAgentListItem>> Handle(ListHumanAgentsQuery request, CancellationToken cancellationToken)
    {
        return await _db.HumanAgents
            .Where(a => a.OwnerUserId == request.OwnerUserId && a.IsActive)
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new HumanAgentListItem(
                a.Id,
                a.Name,
                a.Email,
                a.Status,
                a.IsActive,
                a.MaxConcurrentCalls,
                a.CreatedAt,
                a.UpdatedAt
            ))
            .ToListAsync(cancellationToken);
    }
}
