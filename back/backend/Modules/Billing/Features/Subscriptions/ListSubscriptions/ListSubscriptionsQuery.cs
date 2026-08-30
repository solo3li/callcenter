using System;
using System.Collections.Generic;
using System.Linq;
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

namespace backend.Modules.Billing.Features.Subscriptions.ListSubscriptions;

public record ListSubscriptionsQuery(Guid UserId) : IRequest<List<SubscriptionDto>>;

public class ListSubscriptionsQueryHandler : IRequestHandler<ListSubscriptionsQuery, List<SubscriptionDto>>
{
    private readonly AppDbContext _db;

    public ListSubscriptionsQueryHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<SubscriptionDto>> Handle(ListSubscriptionsQuery request, CancellationToken cancellationToken)
    {
        var subs = await _db.Subscriptions
            .Where(s => s.UserId == request.UserId)
            .Include(s => s.Plan)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(cancellationToken);
        return subs.Select(SubscriptionMapper.Map).ToList();
    }
}
