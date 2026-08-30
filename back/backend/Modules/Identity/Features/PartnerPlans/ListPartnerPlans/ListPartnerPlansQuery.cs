using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediatR;
using backend.Data;
using backend.Dtos;

namespace backend.Modules.Identity.Features.PartnerPlans.ListPartnerPlans;

public record ListPartnerPlansQuery(Guid PartnerId) : IRequest<List<PartnerPlanDto>>;

public class ListPartnerPlansQueryHandler : IRequestHandler<ListPartnerPlansQuery, List<PartnerPlanDto>>
{
    private readonly AppDbContext _db;

    public ListPartnerPlansQueryHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<PartnerPlanDto>> Handle(ListPartnerPlansQuery request, CancellationToken cancellationToken)
    {
        var plans = await _db.PartnerPlans
            .Where(p => p.PartnerId == request.PartnerId)
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
        return plans.Select(PartnerPlanMapper.Map).ToList();
    }
}
