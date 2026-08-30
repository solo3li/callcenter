using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using backend.Dtos;
using backend.Services;

namespace backend.Modules.Analytics.Features.Stats.GetQueueStats;

public record GetQueueStatsQuery(Guid UserId) : IRequest<QueueStatsResponse>;

public class GetQueueStatsQueryHandler : IRequestHandler<GetQueueStatsQuery, QueueStatsResponse>
{
    private readonly StatsService _service;

    public GetQueueStatsQueryHandler(StatsService service)
    {
        _service = service;
    }

    public async Task<QueueStatsResponse> Handle(GetQueueStatsQuery request, CancellationToken cancellationToken)
    {
        return await _service.GetQueueStatsAsync(request.UserId);
    }
}
