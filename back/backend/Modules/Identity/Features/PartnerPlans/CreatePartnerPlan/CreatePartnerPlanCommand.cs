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
using backend.Modules.Identity.Models;

namespace backend.Modules.Identity.Features.PartnerPlans.CreatePartnerPlan;

public record CreatePartnerPlanCommand(Guid PartnerId, CreatePartnerPlanRequest Request) : IRequest<PartnerPlanDto>;

public class CreatePartnerPlanCommandHandler : IRequestHandler<CreatePartnerPlanCommand, PartnerPlanDto>
{
    private readonly AppDbContext _db;

    public CreatePartnerPlanCommandHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<PartnerPlanDto> Handle(CreatePartnerPlanCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var plan = new PartnerPlan
        {
            Id = Guid.NewGuid(),
            PartnerId = command.PartnerId,
            Name = request.Name,
            Description = request.Description,
            IsActive = true,
            EntitlementsJson = request.EntitlementsJson,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.PartnerPlans.Add(plan);
        await _db.SaveChangesAsync(cancellationToken);
        return PartnerPlanMapper.Map(plan);
    }
}
