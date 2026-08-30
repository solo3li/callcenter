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
using backend.Models.Enums;

namespace backend.Modules.Billing.Features.Plans.UpdatePlan;

public record UpdatePlanCommand(Guid Id, UpdatePlanRequest Request) : IRequest<PlanDto?>;

public class UpdatePlanCommandHandler : IRequestHandler<UpdatePlanCommand, PlanDto?>
{
    private readonly AppDbContext _db;

    public UpdatePlanCommandHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<PlanDto?> Handle(UpdatePlanCommand command, CancellationToken cancellationToken)
    {
        var plan = await _db.Plans.FindAsync(new object[] { command.Id }, cancellationToken);
        if (plan == null) return null;

        var request = command.Request;
        if (request.Name != null) plan.Name = request.Name;
        if (request.Description != null) plan.Description = request.Description;
        if (request.Tier != null && Enum.TryParse<PlanTier>(request.Tier, true, out var tier)) plan.Tier = tier;
        if (request.IsActive.HasValue) plan.IsActive = request.IsActive.Value;
        if (request.EntitlementsJson != null) plan.EntitlementsJson = request.EntitlementsJson;
        plan.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);
        return PlanMapper.Map(plan);
    }
}
