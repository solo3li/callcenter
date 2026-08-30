using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using backend.Dtos;
using backend.Services;

namespace backend.Modules.Analytics.Features.Stats.GetTodayStats;

public record GetTodayStatsQuery(Guid UserId) : IRequest<TodayStatsResponse>;

public class GetTodayStatsQueryHandler : IRequestHandler<GetTodayStatsQuery, TodayStatsResponse>
{
    private readonly StatsService _service;

    public GetTodayStatsQueryHandler(StatsService service)
    {
        _service = service;
    }

    public async Task<TodayStatsResponse> Handle(GetTodayStatsQuery request, CancellationToken cancellationToken)
    {
        return await _service.GetTodayStatsAsync(request.UserId);
    }
}
