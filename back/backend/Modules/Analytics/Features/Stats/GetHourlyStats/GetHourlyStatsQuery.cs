using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using backend.Modules.Identity.Dtos;
using backend.Modules.Billing.Dtos;
using backend.Modules.CallOperations.Dtos;
using backend.Modules.Configuration.Dtos;
using backend.Modules.Analytics.Dtos;
using backend.Services;

namespace backend.Modules.Analytics.Features.Stats.GetHourlyStats;

public record GetHourlyStatsQuery(Guid UserId, DateTime? Date) : IRequest<List<HourlyDataPoint>>;

public class GetHourlyStatsQueryHandler : IRequestHandler<GetHourlyStatsQuery, List<HourlyDataPoint>>
{
    private readonly StatsService _service;

    public GetHourlyStatsQueryHandler(StatsService service)
    {
        _service = service;
    }

    public async Task<List<HourlyDataPoint>> Handle(GetHourlyStatsQuery request, CancellationToken cancellationToken)
    {
        return await _service.GetHourlyStatsAsync(request.UserId, request.Date);
    }
}
