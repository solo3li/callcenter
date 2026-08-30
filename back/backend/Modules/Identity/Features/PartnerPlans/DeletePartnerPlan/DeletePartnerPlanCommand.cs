using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using backend.Data;

namespace backend.Modules.Identity.Features.PartnerPlans.DeletePartnerPlan;

public record DeletePartnerPlanCommand(Guid Id) : IRequest<bool>;

public class DeletePartnerPlanCommandHandler : IRequestHandler<DeletePartnerPlanCommand, bool>
{
    private readonly AppDbContext _db;

    public DeletePartnerPlanCommandHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<bool> Handle(DeletePartnerPlanCommand command, CancellationToken cancellationToken)
    {
        var plan = await _db.PartnerPlans.FindAsync(new object[] { command.Id }, cancellationToken);
        if (plan == null) return false;
        
        plan.IsActive = false;
        plan.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
