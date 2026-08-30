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
using backend.Modules.Billing.Models;
using backend.Models.Enums;

namespace backend.Modules.Billing.Features.Plans.CreatePlan;

public record CreatePlanCommand(CreatePlanRequest Request) : IRequest<PlanDto>;

public class CreatePlanCommandHandler : IRequestHandler<CreatePlanCommand, PlanDto>
{
    private readonly AppDbContext _db;

    public CreatePlanCommandHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<PlanDto> Handle(CreatePlanCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        if (!Enum.TryParse<PlanTier>(request.Tier, true, out var tier))
            throw new ArgumentException($"Invalid PlanTier: {request.Tier}");

        var plan = new Plan
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            Tier = tier,
            IsPlatformPlan = request.IsPlatformPlan,
            IsActive = true,
            EntitlementsJson = request.EntitlementsJson,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.Plans.Add(plan);
        await _db.SaveChangesAsync(cancellationToken);
        return PlanMapper.Map(plan);
    }
}
