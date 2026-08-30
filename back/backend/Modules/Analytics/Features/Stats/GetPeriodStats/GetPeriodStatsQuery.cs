using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using backend.Dtos;
using backend.Services;

namespace backend.Modules.Analytics.Features.Stats.GetPeriodStats;

public record GetPeriodStatsQuery(Guid UserId, DateTime From, DateTime To) : IRequest<PeriodStatsResponse>;

public class GetPeriodStatsQueryHandler : IRequestHandler<GetPeriodStatsQuery, PeriodStatsResponse>
{
    private readonly StatsService _service;

    public GetPeriodStatsQueryHandler(StatsService service)
    {
        _service = service;
    }

    public async Task<PeriodStatsResponse> Handle(GetPeriodStatsQuery request, CancellationToken cancellationToken)
    {
        return await _service.GetPeriodStatsAsync(request.UserId, request.From, request.To);
    }
}
