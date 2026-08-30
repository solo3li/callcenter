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

namespace backend.Modules.Analytics.Features.Stats.GetIntentStats;

public record GetIntentStatsQuery(Guid UserId, DateTime? From, DateTime? To) : IRequest<List<IntentStatsDto>>;

public class GetIntentStatsQueryHandler : IRequestHandler<GetIntentStatsQuery, List<IntentStatsDto>>
{
    private readonly StatsService _service;

    public GetIntentStatsQueryHandler(StatsService service)
    {
        _service = service;
    }

    public async Task<List<IntentStatsDto>> Handle(GetIntentStatsQuery request, CancellationToken cancellationToken)
    {
        return await _service.GetIntentStatsAsync(request.UserId, request.From, request.To);
    }
}
