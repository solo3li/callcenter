using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using backend.Data;
using backend.Modules.Identity.Dtos;
using backend.Modules.Billing.Dtos;
using backend.Modules.CallOperations.Dtos;
using backend.Modules.Configuration.Dtos;
using backend.Modules.Analytics.Dtos;

namespace backend.Modules.Identity.Features.PartnerPlans.GetPartnerPlan;

public record GetPartnerPlanQuery(Guid Id) : IRequest<PartnerPlanDto?>;

public class GetPartnerPlanQueryHandler : IRequestHandler<GetPartnerPlanQuery, PartnerPlanDto?>
{
    private readonly AppDbContext _db;

    public GetPartnerPlanQueryHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<PartnerPlanDto?> Handle(GetPartnerPlanQuery request, CancellationToken cancellationToken)
    {
        var plan = await _db.PartnerPlans.FindAsync(new object[] { request.Id }, cancellationToken);
        return plan == null ? null : PartnerPlanMapper.Map(plan);
    }
}
