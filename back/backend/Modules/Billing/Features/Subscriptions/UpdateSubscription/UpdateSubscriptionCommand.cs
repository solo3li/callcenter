using System;
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
using backend.Models.Enums;

namespace backend.Modules.Billing.Features.Subscriptions.UpdateSubscription;

public record UpdateSubscriptionCommand(Guid Id, UpdateSubscriptionRequest Request) : IRequest<SubscriptionDto?>;

public class UpdateSubscriptionCommandHandler : IRequestHandler<UpdateSubscriptionCommand, SubscriptionDto?>
{
    private readonly AppDbContext _db;

    public UpdateSubscriptionCommandHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<SubscriptionDto?> Handle(UpdateSubscriptionCommand command, CancellationToken cancellationToken)
    {
        var sub = await _db.Subscriptions
            .Include(s => s.Plan)
            .FirstOrDefaultAsync(s => s.Id == command.Id, cancellationToken);
        if (sub == null) return null;

        var request = command.Request;
        if (request.Status != null && Enum.TryParse<SubscriptionStatus>(request.Status, true, out var status))
            sub.Status = status;
        if (request.EndsAt.HasValue)
            sub.EndsAt = request.EndsAt.Value;
        sub.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);
        return SubscriptionMapper.Map(sub);
    }
}
