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

namespace backend.Modules.Billing.Features.Usage.GetUsageByCall;

public record GetUsageByCallQuery(Guid UserId, Guid CallSessionId) : IRequest<List<UsageRecordDto>>;

public class GetUsageByCallQueryHandler : IRequestHandler<GetUsageByCallQuery, List<UsageRecordDto>>
{
    private readonly AppDbContext _db;

    public GetUsageByCallQueryHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<UsageRecordDto>> Handle(GetUsageByCallQuery request, CancellationToken cancellationToken)
    {
        var records = await _db.UsageRecords
            .Where(u => u.UserId == request.UserId && u.CallSessionId == request.CallSessionId)
            .OrderByDescending(u => u.OccurredAt)
            .ToListAsync(cancellationToken);

        return records.Select(UsageMapper.Map).ToList();
    }
}
