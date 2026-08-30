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
using backend.Models.Enums;

namespace backend.Modules.Billing.Features.Usage.ListUsage;

public record ListUsageQuery(Guid UserId, UsageFilterRequest? Filter = null) : IRequest<List<UsageRecordDto>>;

public class ListUsageQueryHandler : IRequestHandler<ListUsageQuery, List<UsageRecordDto>>
{
    private readonly AppDbContext _db;

    public ListUsageQueryHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<UsageRecordDto>> Handle(ListUsageQuery request, CancellationToken cancellationToken)
    {
        var filter = request.Filter;
        var query = _db.UsageRecords.Where(u => u.UserId == request.UserId);

        if (filter?.From.HasValue == true)
            query = query.Where(u => u.OccurredAt >= filter.From.Value);
        if (filter?.To.HasValue == true)
            query = query.Where(u => u.OccurredAt <= filter.To.Value);
        if (!string.IsNullOrEmpty(filter?.MetricType) && Enum.TryParse<MetricType>(filter.MetricType, true, out var mt))
            query = query.Where(u => u.MetricType == mt);
        if (filter?.CallSessionId.HasValue == true)
            query = query.Where(u => u.CallSessionId == filter.CallSessionId.Value);
        if (filter?.LicenseId.HasValue == true)
            query = query.Where(u => u.LicenseId == filter.LicenseId.Value);
        if (filter?.PartnerId.HasValue == true)
            query = query.Where(u => u.PartnerId == filter.PartnerId.Value);

        var records = await query.OrderByDescending(u => u.OccurredAt).Take(500).ToListAsync(cancellationToken);
        return records.Select(UsageMapper.Map).ToList();
    }
}
