using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using backend.Dtos;
using backend.Services;

namespace backend.Modules.Analytics.Features.Stats.GetSummaryStats;

public record GetSummaryStatsQuery(Guid UserId) : IRequest<SummaryStatsResponse>;

public class GetSummaryStatsQueryHandler : IRequestHandler<GetSummaryStatsQuery, SummaryStatsResponse>
{
    private readonly StatsService _service;

    public GetSummaryStatsQueryHandler(StatsService service)
    {
        _service = service;
    }

    public async Task<SummaryStatsResponse> Handle(GetSummaryStatsQuery request, CancellationToken cancellationToken)
    {
        return await _service.GetSummaryStatsAsync(request.UserId);
    }
}
