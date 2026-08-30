using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediatR;
using backend.Data;
using backend.Models.Domain;
using backend.Models.Enums;

namespace backend.Modules.CallOperations.Features.CallSessions.CreateCallSession;

public record CreateCallSessionCommand(Guid UserId, Guid? CallConfigurationId, Guid? PersonaVersionId, string RoomName, string Direction) : IRequest<CallSession>;

public class CreateCallSessionCommandHandler : IRequestHandler<CreateCallSessionCommand, CallSession>
{
    private readonly AppDbContext _db;

    public CreateCallSessionCommandHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<CallSession> Handle(CreateCallSessionCommand request, CancellationToken cancellationToken)
    {
        var parsedDirection = Enum.TryParse<CallDirection>(request.Direction, true, out var d)
            ? d
            : CallDirection.Inbound;

        var session = new CallSession
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            CallConfigurationId = request.CallConfigurationId,
            PersonaVersionId = request.PersonaVersionId,
            LivekitRoomName = request.RoomName,
            Status = CallSessionStatus.Queued,
            Direction = parsedDirection,
            StartedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        _db.CallSessions.Add(session);
        await _db.SaveChangesAsync(cancellationToken);

        return session;
    }
}

public static class CallSessionHelpers
{
    public static string GenerateRoomName()
    {
        var bytes = RandomNumberGenerator.GetBytes(4);
        var hex = new StringBuilder(8);
        foreach (var b in bytes)
            hex.Append(b.ToString("x2"));
        return $"call_{hex}";
    }
}
