using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediatR;
using backend.Data;
using backend.Models.Enums;
using backend.Services;

namespace backend.Modules.CallOperations.Features.LiveKit.GenerateLiveKitToken;

public record GenerateLiveKitTokenCommand(
    string Identity, 
    string RoomName, 
    bool CanPublish, 
    bool CanSubscribe) : IRequest<string>;

public class GenerateLiveKitTokenCommandHandler : IRequestHandler<GenerateLiveKitTokenCommand, string>
{
    private readonly AppDbContext _db;
    private readonly LiveKitService _service;

    public GenerateLiveKitTokenCommandHandler(AppDbContext db, LiveKitService service)
    {
        _db = db;
        _service = service;
    }

    public async Task<string> Handle(GenerateLiveKitTokenCommand request, CancellationToken cancellationToken)
    {
        if (request.Identity.StartsWith("agent_"))
        {
            var agentIdStr = request.Identity["agent_".Length..];
            if (!Guid.TryParse(agentIdStr, out var agentId))
                throw new InvalidOperationException("Invalid agent identity");

            var hasTransfer = await _db.CallTransfers.AnyAsync(t =>
                t.Status == CallTransferStatus.Accepted &&
                t.ToHumanAgentId == agentId &&
                t.CallSession.LivekitRoomName == request.RoomName, cancellationToken);

            if (!hasTransfer)
                throw new UnauthorizedAccessException("Forbidden");
        }

        return _service.GenerateToken(
            request.Identity,
            request.RoomName,
            request.CanPublish,
            request.CanSubscribe);
    }
}
