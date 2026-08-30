using System;
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

namespace backend.Modules.CallOperations.Features.HumanAgents.GetHumanAgent;

public record GetHumanAgentQuery(Guid Id, Guid OwnerUserId) : IRequest<HumanAgentListItem?>;

public class GetHumanAgentQueryHandler : IRequestHandler<GetHumanAgentQuery, HumanAgentListItem?>
{
    private readonly AppDbContext _db;

    public GetHumanAgentQueryHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<HumanAgentListItem?> Handle(GetHumanAgentQuery request, CancellationToken cancellationToken)
    {
        return await _db.HumanAgents
            .Where(a => a.Id == request.Id && a.OwnerUserId == request.OwnerUserId && a.IsActive)
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
            .FirstOrDefaultAsync(cancellationToken);
    }
}
