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

namespace backend.Modules.CallOperations.Features.CallSessions.ListCallParticipants;

public record ListCallParticipantsQuery(Guid CallSessionId, Guid UserId) : IRequest<List<CallParticipantDto>>;

public class ListCallParticipantsQueryHandler : IRequestHandler<ListCallParticipantsQuery, List<CallParticipantDto>>
{
    private readonly AppDbContext _db;

    public ListCallParticipantsQueryHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<CallParticipantDto>> Handle(ListCallParticipantsQuery request, CancellationToken cancellationToken)
    {
        var owned = await _db.CallSessions
            .AnyAsync(c => c.Id == request.CallSessionId && c.UserId == request.UserId, cancellationToken);

        if (!owned)
            return new List<CallParticipantDto>();

        return await _db.CallParticipants
            .Where(p => p.CallSessionId == request.CallSessionId)
            .Select(p => new CallParticipantDto(
                p.Id,
                p.HumanAgentId,
                p.ParticipantType.ToString(),
                p.LivekitIdentity,
                p.LivekitParticipantSid,
                p.DisplayName,
                p.JoinedAt,
                p.LeftAt,
                p.CreatedAt
            ))
            .ToListAsync(cancellationToken);
    }
}
