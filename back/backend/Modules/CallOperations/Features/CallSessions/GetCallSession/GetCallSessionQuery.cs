using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediatR;
using backend.Data;
using backend.Dtos;

namespace backend.Modules.CallOperations.Features.CallSessions.GetCallSession;

public record GetCallSessionQuery(Guid Id, Guid UserId) : IRequest<CallSessionDetail?>;

public class GetCallSessionQueryHandler : IRequestHandler<GetCallSessionQuery, CallSessionDetail?>
{
    private readonly AppDbContext _db;

    public GetCallSessionQueryHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<CallSessionDetail?> Handle(GetCallSessionQuery request, CancellationToken cancellationToken)
    {
        var session = await _db.CallSessions
            .Where(c => c.Id == request.Id && c.UserId == request.UserId)
            .Include(c => c.Participants)
            .Include(c => c.Transfers).ThenInclude(t => t.ToHumanAgent)
            .Include(c => c.Recordings)
            .Include(c => c.Handoffs).ThenInclude(h => h.ToHumanAgent)
            .Include(c => c.CallConfiguration)
            .FirstOrDefaultAsync(cancellationToken);

        if (session == null)
            return null;

        return new CallSessionDetail(
            session.Id,
            session.UserId,
            session.CallConfigurationId,
            session.CallConfiguration?.Name,
            session.PersonaVersionId,
            session.WorkflowVersionId,
            session.ApiKeyId,
            session.LivekitRoomName,
            session.LivekitRoomSid,
            session.Status.ToString(),
            session.Direction.ToString(),
            session.StartedAt,
            session.AnsweredAt,
            session.EndedAt,
            session.DurationSeconds,
            session.MetadataJson,
            session.CreatedAt,
            session.Participants.Select(p => new CallParticipantDto(
                p.Id,
                p.HumanAgentId,
                p.ParticipantType.ToString(),
                p.LivekitIdentity,
                p.LivekitParticipantSid,
                p.DisplayName,
                p.JoinedAt,
                p.LeftAt,
                p.CreatedAt
            )).ToList(),
            session.Transfers.Select(t => new CallTransferDetailDto(
                t.Id,
                t.CallSessionId,
                t.FromParticipantId,
                t.ToHumanAgentId,
                t.ToHumanAgent != null
                    ? t.ToHumanAgent.Name
                    : (t.Destination != null ? t.Destination.Name : "External"),
                t.Status.ToString(),
                t.Reason,
                t.FailureReason,
                t.RequestedAt,
                t.AcceptedAt,
                t.CompletedAt,
                t.FailedAt
            )).ToList(),
            session.Recordings.Select(r => new CallRecordingDetailDto(
                r.Id,
                r.StorageProvider,
                r.ObjectKey,
                r.ContentType,
                r.DurationSeconds,
                r.SizeBytes,
                r.Status.ToString(),
                r.CreatedAt,
                r.CompletedAt
            )).ToList(),
            session.Handoffs.Select(h => new CallHandoffDetailDto(
                h.Id,
                h.CallTransferId,
                h.FromParticipantId,
                h.ToHumanAgentId,
                h.ToHumanAgent!.Name,
                h.Reason,
                h.Summary,
                h.ContextDataJson,
                h.Status.ToString(),
                h.CreatedAt,
                h.DeliveredAt,
                h.AcceptedAt
            )).FirstOrDefault()
        );
    }
}
