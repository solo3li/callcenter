using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediatR;
using backend.Data;
using backend.Dtos;
using backend.Models.Enums;

namespace backend.Modules.Billing.Features.Usage.GetUsageByMetricType;

public record GetUsageByMetricTypeQuery(Guid UserId, string MetricType) : IRequest<List<UsageRecordDto>>;

public class GetUsageByMetricTypeQueryHandler : IRequestHandler<GetUsageByMetricTypeQuery, List<UsageRecordDto>>
{
    private readonly AppDbContext _db;

    public GetUsageByMetricTypeQueryHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<UsageRecordDto>> Handle(GetUsageByMetricTypeQuery request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<MetricType>(request.MetricType, true, out var mt))
            return new List<UsageRecordDto>();

        var records = await _db.UsageRecords
            .Where(u => u.UserId == request.UserId && u.MetricType == mt)
            .OrderByDescending(u => u.OccurredAt)
            .Take(500)
            .ToListAsync(cancellationToken);

        return records.Select(UsageMapper.Map).ToList();
    }
}
