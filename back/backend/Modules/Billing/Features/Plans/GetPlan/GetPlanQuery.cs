using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using backend.Data;
using backend.Dtos;

namespace backend.Modules.Billing.Features.Plans.GetPlan;

public record GetPlanQuery(Guid Id) : IRequest<PlanDto?>;

public class GetPlanQueryHandler : IRequestHandler<GetPlanQuery, PlanDto?>
{
    private readonly AppDbContext _db;

    public GetPlanQueryHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<PlanDto?> Handle(GetPlanQuery request, CancellationToken cancellationToken)
    {
        var plan = await _db.Plans.FindAsync(new object[] { request.Id }, cancellationToken);
        return plan == null ? null : PlanMapper.Map(plan);
    }
}
