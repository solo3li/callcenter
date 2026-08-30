using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using backend.Data;
using backend.Models.Enums;

namespace backend.Modules.Billing.Features.Subscriptions.CancelSubscription;

public record CancelSubscriptionCommand(Guid Id) : IRequest<bool>;

public class CancelSubscriptionCommandHandler : IRequestHandler<CancelSubscriptionCommand, bool>
{
    private readonly AppDbContext _db;

    public CancelSubscriptionCommandHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<bool> Handle(CancelSubscriptionCommand command, CancellationToken cancellationToken)
    {
        var sub = await _db.Subscriptions.FindAsync(new object[] { command.Id }, cancellationToken);
        if (sub == null) return false;
        
        sub.Status = SubscriptionStatus.Cancelled;
        sub.EndsAt = DateTime.UtcNow;
        sub.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
