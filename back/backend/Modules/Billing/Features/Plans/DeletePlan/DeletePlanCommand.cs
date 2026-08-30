using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using backend.Data;

namespace backend.Modules.Billing.Features.Plans.DeletePlan;

public record DeletePlanCommand(Guid Id) : IRequest<bool>;

public class DeletePlanCommandHandler : IRequestHandler<DeletePlanCommand, bool>
{
    private readonly AppDbContext _db;

    public DeletePlanCommandHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<bool> Handle(DeletePlanCommand command, CancellationToken cancellationToken)
    {
        var plan = await _db.Plans.FindAsync(new object[] { command.Id }, cancellationToken);
        if (plan == null) return false;
        
        plan.IsActive = false;
        plan.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
