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

namespace backend.Modules.Billing.Features.Plans.ListPlans;

public record ListPlansQuery(bool OnlyActive = false) : IRequest<List<PlanDto>>;

public class ListPlansQueryHandler : IRequestHandler<ListPlansQuery, List<PlanDto>>
{
    private readonly AppDbContext _db;

    public ListPlansQueryHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<PlanDto>> Handle(ListPlansQuery request, CancellationToken cancellationToken)
    {
        var query = _db.Plans.AsQueryable();
        if (request.OnlyActive)
            query = query.Where(p => p.IsActive);
            
        var plans = await query.OrderBy(p => p.Name).ToListAsync(cancellationToken);
        return plans.Select(PlanMapper.Map).ToList();
    }
}
