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

namespace backend.Modules.Analytics.Features.Stats.GetAgentStats;

public record GetAgentStatsQuery(Guid UserId) : IRequest<List<AgentStatsDto>>;

public class GetAgentStatsQueryHandler : IRequestHandler<GetAgentStatsQuery, List<AgentStatsDto>>
{
    private readonly StatsService _service;

    public GetAgentStatsQueryHandler(StatsService service)
    {
        _service = service;
    }

    public async Task<List<AgentStatsDto>> Handle(GetAgentStatsQuery request, CancellationToken cancellationToken)
    {
        return await _service.GetAgentStatsAsync(request.UserId);
    }
}
