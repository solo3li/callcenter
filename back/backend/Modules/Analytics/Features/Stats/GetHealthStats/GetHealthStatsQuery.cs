using System.Threading;
using System.Threading.Tasks;
using MediatR;
using backend.Services;

namespace backend.Modules.Analytics.Features.Stats.GetHealthStats;

public record GetHealthStatsQuery() : IRequest<object>;

public class GetHealthStatsQueryHandler : IRequestHandler<GetHealthStatsQuery, object>
{
    private readonly StatsService _service;

    public GetHealthStatsQueryHandler(StatsService service)
    {
        _service = service;
    }

    public async Task<object> Handle(GetHealthStatsQuery request, CancellationToken cancellationToken)
    {
        return await _service.GetHealthAsync();
    }
}
