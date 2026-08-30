using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediatR;
using backend.Data;
using backend.Dtos;
using backend.Models.Enums;

namespace backend.Modules.CallOperations.Features.CallSessions.EndCallSession;

public record EndCallSessionCommand(Guid Id, Guid UserId) : IRequest<EndCallResponse?>;

public class EndCallSessionCommandHandler : IRequestHandler<EndCallSessionCommand, EndCallResponse?>
{
    private readonly AppDbContext _db;

    public EndCallSessionCommandHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<EndCallResponse?> Handle(EndCallSessionCommand request, CancellationToken cancellationToken)
    {
        var session = await _db.CallSessions
            .FirstOrDefaultAsync(c => c.Id == request.Id && c.UserId == request.UserId, cancellationToken);

        if (session == null)
            return null;

        var now = DateTime.UtcNow;
        session.EndedAt = now;
        session.DurationSeconds = (int)(now - session.StartedAt).TotalSeconds;
        session.Status = CallSessionStatus.Completed;
        await _db.SaveChangesAsync(cancellationToken);

        return new EndCallResponse(
            session.Id,
            session.Status.ToString(),
            session.DurationSeconds,
            session.EndedAt
        );
    }
}
