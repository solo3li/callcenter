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

namespace backend.Modules.Billing.Features.Subscriptions.GetSubscription;

public record GetSubscriptionQuery(Guid Id) : IRequest<SubscriptionDto?>;

public class GetSubscriptionQueryHandler : IRequestHandler<GetSubscriptionQuery, SubscriptionDto?>
{
    private readonly AppDbContext _db;

    public GetSubscriptionQueryHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<SubscriptionDto?> Handle(GetSubscriptionQuery request, CancellationToken cancellationToken)
    {
        var sub = await _db.Subscriptions
            .Include(s => s.Plan)
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);
        return sub == null ? null : SubscriptionMapper.Map(sub);
    }
}
