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
using backend.Models.Enums;

namespace backend.Modules.CallOperations.Features.CallSessions.GetActiveCalls;

public record GetActiveCallsQuery(Guid UserId) : IRequest<List<ActiveCallDto>>;

public class GetActiveCallsQueryHandler : IRequestHandler<GetActiveCallsQuery, List<ActiveCallDto>>
{
    private readonly AppDbContext _db;

    public GetActiveCallsQueryHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<ActiveCallDto>> Handle(GetActiveCallsQuery request, CancellationToken cancellationToken)
    {
        var activeStatuses = new List<CallSessionStatus>
        {
            CallSessionStatus.Queued,
            CallSessionStatus.Ringing,
            CallSessionStatus.Active,
            CallSessionStatus.Transferred
        };

        return await _db.CallSessions
            .Where(c => c.UserId == request.UserId && activeStatuses.Contains(c.Status))
            .OrderByDescending(c => c.StartedAt)
            .Select(c => new ActiveCallDto(
                c.Id,
                c.LivekitRoomName,
                c.Status.ToString(),
                c.Direction.ToString(),
                c.StartedAt,
                c.AnsweredAt,
                c.DurationSeconds,
                _db.CallParticipants.Count(p => p.CallSessionId == c.Id),
                c.CreatedAt
            ))
            .ToListAsync(cancellationToken);
    }
}
