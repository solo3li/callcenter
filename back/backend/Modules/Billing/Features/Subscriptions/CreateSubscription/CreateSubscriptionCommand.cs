using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using backend.Data;
using backend.Dtos;
using backend.Modules.Billing.Models;
using backend.Models.Enums;

namespace backend.Modules.Billing.Features.Subscriptions.CreateSubscription;

public record CreateSubscriptionCommand(Guid UserId, CreateSubscriptionRequest Request) : IRequest<SubscriptionDto>;

public class CreateSubscriptionCommandHandler : IRequestHandler<CreateSubscriptionCommand, SubscriptionDto>
{
    private readonly AppDbContext _db;

    public CreateSubscriptionCommandHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<SubscriptionDto> Handle(CreateSubscriptionCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var plan = await _db.Plans.FindAsync(new object[] { request.PlanId }, cancellationToken);
        if (plan == null)
            throw new ArgumentException("Plan not found");

        var sub = new Subscription
        {
            Id = Guid.NewGuid(),
            UserId = command.UserId,
            PlanId = request.PlanId,
            Status = SubscriptionStatus.Trialing,
            StartsAt = request.StartsAt,
            EndsAt = request.EndsAt,
            TrialEndsAt = request.TrialEndsAt,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.Subscriptions.Add(sub);
        await _db.SaveChangesAsync(cancellationToken);

        await _db.Entry(sub).Reference(s => s.Plan).LoadAsync(cancellationToken);
        return SubscriptionMapper.Map(sub);
    }
}
