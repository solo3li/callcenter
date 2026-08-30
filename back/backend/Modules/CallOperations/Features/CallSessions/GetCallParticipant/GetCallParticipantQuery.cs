using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediatR;
using backend.Data;
using backend.Dtos;

namespace backend.Modules.CallOperations.Features.CallSessions.GetCallParticipant;

public record GetCallParticipantQuery(Guid CallSessionId, Guid ParticipantId, Guid UserId) : IRequest<CallParticipantDto?>;

public class GetCallParticipantQueryHandler : IRequestHandler<GetCallParticipantQuery, CallParticipantDto?>
{
    private readonly AppDbContext _db;

    public GetCallParticipantQueryHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<CallParticipantDto?> Handle(GetCallParticipantQuery request, CancellationToken cancellationToken)
    {
        var owned = await _db.CallSessions
            .AnyAsync(c => c.Id == request.CallSessionId && c.UserId == request.UserId, cancellationToken);

        if (!owned)
            return null;

        return await _db.CallParticipants
            .Where(p => p.CallSessionId == request.CallSessionId && p.Id == request.ParticipantId)
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
            .FirstOrDefaultAsync(cancellationToken);
    }
}
