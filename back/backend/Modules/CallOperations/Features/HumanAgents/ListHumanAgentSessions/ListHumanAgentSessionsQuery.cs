using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediatR;
using backend.Data;
using backend.Dtos;

namespace backend.Modules.CallOperations.Features.HumanAgents.ListHumanAgentSessions;

public record ListHumanAgentSessionsQuery(Guid HumanAgentId, Guid OwnerUserId) : IRequest<List<AgentSessionDto>>;

public class ListHumanAgentSessionsQueryHandler : IRequestHandler<ListHumanAgentSessionsQuery, List<AgentSessionDto>>
{
    private readonly AppDbContext _db;

    public ListHumanAgentSessionsQueryHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<AgentSessionDto>> Handle(ListHumanAgentSessionsQuery request, CancellationToken cancellationToken)
    {
        var agent = await _db.HumanAgents
            .FirstOrDefaultAsync(a => a.Id == request.HumanAgentId && a.OwnerUserId == request.OwnerUserId && a.IsActive, cancellationToken);

        if (agent is null)
            return new List<AgentSessionDto>();

        return await _db.HumanAgentSessions
            .Where(s => s.HumanAgentId == request.HumanAgentId)
            .OrderByDescending(s => s.ConnectedAt)
            .Select(s => new AgentSessionDto(
                s.Id,
                s.HumanAgentId,
                s.LivekitIdentity,
                s.Status,
                s.ConnectedAt,
                s.DisconnectedAt,
                s.LastHeartbeatAt,
                s.MetadataJson
            ))
            .ToListAsync(cancellationToken);
    }
}
