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

namespace backend.Modules.Billing.Features.Usage.GetUsageSummary;

public record GetUsageSummaryQuery(Guid UserId) : IRequest<List<UsageSummaryDto>>;

public class GetUsageSummaryQueryHandler : IRequestHandler<GetUsageSummaryQuery, List<UsageSummaryDto>>
{
    private readonly AppDbContext _db;

    public GetUsageSummaryQueryHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<UsageSummaryDto>> Handle(GetUsageSummaryQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var summaries = await _db.UsageRecords
            .Where(u => u.UserId == request.UserId && u.OccurredAt >= monthStart)
            .GroupBy(u => new { u.MetricType, u.Unit })
            .Select(g => new UsageSummaryDto(
                g.Key.MetricType.ToString(),
                g.Sum(u => u.Quantity),
                g.Key.Unit,
                g.Count()
            ))
            .ToListAsync(cancellationToken);

        return summaries;
    }
}
