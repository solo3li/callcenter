using System.Text.Json;
using backend.Data;
using backend.Models.Domain;
using backend.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

/// <summary>
/// Drives the v0 inbound call lifecycle from LiveKit server webhooks.
///
/// Room naming contract: SIP dispatch rules create individual rooms prefixed
/// "call_u{ownerUserId}", letting every component resolve ownership from the
/// room name without extra lookups.
///
/// Participant identity conventions:
///   caller            any other identity (SIP participant)
///   "ai-agent"        python-ai-worker
///   "agent_{id}"      internal human agent app (WebRTC)
///   "dest_{guid}"     bridged external PBX destination leg
/// </summary>
public class InboundRoutingService
{
    public const string AiIdentity = "ai-agent";
    public const string AgentIdentityPrefix = "agent_";
    public const string DestinationIdentityPrefix = "dest_";
    public const string RoomOwnerPrefix = "call_u";
    public const string DispatchAgentName = "voice-agent";

    private readonly AppDbContext _db;
    private readonly LiveKitService _liveKit;
    private readonly ILogger<InboundRoutingService> _logger;

    public InboundRoutingService(AppDbContext db, LiveKitService liveKit,
        ILogger<InboundRoutingService> logger)
    {
        _db = db;
        _liveKit = liveKit;
        _logger = logger;
    }

    public async Task HandleParticipantJoinedAsync(string roomName, string identity)
    {
        var ownerUserId = TryGetOwner(roomName);
        if (ownerUserId == null)
            return; // not a platform-managed SIP room

        if (identity == AiIdentity)
        {
            await MarkAiAnsweredAsync(roomName);
            return;
        }

        if (identity.StartsWith(AgentIdentityPrefix, StringComparison.Ordinal))
        {
            await CompleteHumanSwapAsync(roomName, identity);
            return;
        }

        if (identity.StartsWith(DestinationIdentityPrefix, StringComparison.Ordinal))
        {
            await CompleteDestinationSwapAsync(roomName, identity);
            return;
        }

        // Otherwise: the caller's first arrival — create session, route to AI.
        await RouteIncomingCallAsync(roomName, ownerUserId.Value, identity);
    }

    public async Task HandleParticipantLeftAsync(string roomName, string identity)
    {
        var session = await _db.CallSessions.FirstOrDefaultAsync(c => c.LivekitRoomName == roomName);
        if (session == null)
            return;

        await CloseLegAsync(session.Id, identity);

        bool callerLeft = !identity.StartsWith(AgentIdentityPrefix, StringComparison.Ordinal)
            && !identity.StartsWith(DestinationIdentityPrefix, StringComparison.Ordinal)
            && identity != AiIdentity;

        if (callerLeft && session.EndedAt == null)
            await EndSessionAsync(session, hangupCause: "caller_left");
    }

    public async Task HandleRoomFinishedAsync(string roomName)
    {
        var session = await _db.CallSessions.FirstOrDefaultAsync(c => c.LivekitRoomName == roomName);
        if (session != null && session.EndedAt == null)
            await EndSessionAsync(session, hangupCause: "room_finished");
    }

    // ── routing ───────────────────────────────────────────────────────────

    private async Task RouteIncomingCallAsync(string roomName, Guid ownerUserId, string callerIdentity)
    {
        var user = await _db.Users.Include(u => u.DefaultPersona)
            .FirstOrDefaultAsync(u => u.Id == ownerUserId);
        if (user == null)
        {
            _logger.LogWarning("Inbound call in {Room}: owner {User} not found", roomName, ownerUserId);
            return;
        }

        var session = await _db.CallSessions.FirstOrDefaultAsync(c => c.LivekitRoomName == roomName);
        if (session == null)
        {
            var originConn = await ResolveOriginConnectionAsync(ownerUserId);
            session = new CallSession
            {
                Id = Guid.NewGuid(),
                UserId = ownerUserId,
                LivekitRoomName = roomName,
                Direction = CallDirection.Inbound,
                Status = CallSessionStatus.Ringing,
                StartedAt = DateTime.UtcNow,
                OriginSipConnectionId = originConn?.Id,
                DialedNumber = originConn?.Numbers.FirstOrDefault()
            };
            _db.CallSessions.Add(session);
            await _db.SaveChangesAsync();
        }
        else if (!string.IsNullOrEmpty(session.MetadataJson) &&
                 session.MetadataJson.Contains("\"dispatched\":true"))
        {
            return; // already routed; duplicate delivery
        }

        await OpenLegAsync(session.Id, CallLegKind.PstnIn, callerIdentity);

        var personaVersion = await ResolvePublishedPersonaVersionAsync(user.DefaultPersonaId);
        if (personaVersion == null)
        {
            _logger.LogWarning("NoRoute: user {User} has no default persona with a published version", ownerUserId);
            session.Status = CallSessionStatus.Failed;
            session.MetadataJson = JsonSerializer.Serialize(new { dispatched = true, route = "no_route" });
            await _db.SaveChangesAsync();
            await _liveKit.RemoveParticipant(roomName, callerIdentity);
            return;
        }

        try
        {
            var metadata = JsonSerializer.Serialize(new
            {
                sessionId = session.Id,
                personaId = personaVersion.PersonaId
            });
            await _liveKit.CreateAgentDispatch(roomName, DispatchAgentName, metadata);
            session.MetadataJson = JsonSerializer.Serialize(new
            {
                dispatched = true,
                personaVersionId = personaVersion.Id
            });
            session.PersonaVersionId = personaVersion.Id;
            await _db.SaveChangesAsync();
            _logger.LogInformation("Dispatched {Agent} to {Room} (session {Session})",
                DispatchAgentName, roomName, session.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CreateAgentDispatch failed for {Room}", roomName);
            session.Status = CallSessionStatus.Failed;
            await _db.SaveChangesAsync();
        }
    }

    private async Task<PersonaVersion?> ResolvePublishedPersonaVersionAsync(Guid? defaultPersonaId)
    {
        if (defaultPersonaId == null) return null;
        return await _db.PersonaVersions
            .Where(v => v.PersonaId == defaultPersonaId && v.IsPublished)
            .OrderByDescending(v => v.VersionNumber)
            .FirstOrDefaultAsync();
    }

    private async Task<SipConnection?> ResolveOriginConnectionAsync(Guid ownerUserId)
    {
        return await _db.SipConnections
            .Where(c => c.UserId == ownerUserId && c.IsActive)
            .OrderBy(c => c.CreatedAt)
            .FirstOrDefaultAsync();
    }

    // ── swaps ─────────────────────────────────────────────────────────────

    private async Task CompleteHumanSwapAsync(string roomName, string agentIdentity)
    {
        var agentIdText = agentIdentity[AgentIdentityPrefix.Length..];
        if (!Guid.TryParse(agentIdText, out var agentId)) return;

        var session = await _db.CallSessions.FirstOrDefaultAsync(c => c.LivekitRoomName == roomName);
        if (session == null) return;

        var transfer = await _db.CallTransfers
            .Include(t => t.ToHumanAgent)
            .Where(t => t.CallSessionId == session.Id
                && t.TargetType == TransferTargetType.HumanAgent
                && t.Status != CallTransferStatus.Completed
                && t.Status != CallTransferStatus.Cancelled)
            .OrderByDescending(t => t.RequestedAt)
            .FirstOrDefaultAsync();

        if (transfer == null || (transfer.ToHumanAgentId != agentId)) return;

        await SwapToLegAsync(session, transfer, CallLegKind.WebrtcAgent, agentIdentity,
            agentSnapshotName: transfer.ToHumanAgent?.Name);
    }

    private async Task CompleteDestinationSwapAsync(string roomName, string destIdentity)
    {
        var destIdText = destIdentity[DestinationIdentityPrefix.Length..];
        if (!Guid.TryParse(destIdText, out var destId)) return;

        var session = await _db.CallSessions.FirstOrDefaultAsync(c => c.LivekitRoomName == roomName);
        if (session == null) return;

        var transfer = await _db.CallTransfers
            .Where(t => t.CallSessionId == session.Id
                && t.TargetType == TransferTargetType.ExternalDestination
                && t.DestinationId == destId
                && t.Status != CallTransferStatus.Completed
                && t.Status != CallTransferStatus.Cancelled)
            .OrderByDescending(t => t.RequestedAt)
            .FirstOrDefaultAsync();

        if (transfer == null) return;

        var destName = await _db.SipDestinations.Where(d => d.Id == destId)
            .Select(d => d.Name).FirstOrDefaultAsync();

        await SwapToLegAsync(session, transfer, CallLegKind.SipExternal, destIdentity, destName);
    }

    /// <summary>Cold silent swap: new party's leg opens, AI leg closes, AI removed.</summary>
    private async Task SwapToLegAsync(CallSession session, CallTransfer transfer,
        CallLegKind kind, string identity, string? agentSnapshotName = null)
    {
        transfer.Status = CallTransferStatus.Completed;
        transfer.CompletedAt = DateTime.UtcNow;
        transfer.UpdatedAt = DateTime.UtcNow;

        if (transfer.TargetType == TransferTargetType.HumanAgent && transfer.ToHumanAgent != null)
            transfer.ToHumanAgent.Status = HumanAgentStatus.InCall;

        var handoff = await _db.CallHandoffs.FirstOrDefaultAsync(h => h.CallTransferId == transfer.Id);
        if (handoff != null && handoff.Status == HandoffStatus.Pending)
        {
            handoff.Status = HandoffStatus.Accepted;
            handoff.AcceptedAt = DateTime.UtcNow;
        }

        await OpenLegAsync(session.Id, kind, identity, answered: true);

        session.Status = CallSessionStatus.Transferred;
        await _db.SaveChangesAsync();

        var fromIdentity = ExtractFromIdentity(transfer.TargetSnapshotJson);
        await CloseLegByIdentityAsync(session.Id, fromIdentity, "swapped_out");
        await _liveKit.RemoveParticipant(session.LivekitRoomName, fromIdentity);

        _logger.LogInformation("Cold swap completed for session {Session} -> {Identity} ({Kind}), removed {From}",
            session.Id, identity, kind, fromIdentity);
    }

    private async Task MarkAiAnsweredAsync(string roomName)
    {
        var session = await _db.CallSessions.FirstOrDefaultAsync(c => c.LivekitRoomName == roomName);
        if (session == null) return;

        session.Status = CallSessionStatus.Active;
        session.AnsweredAt ??= DateTime.UtcNow;
        await _db.SaveChangesAsync();

        var leg = await _db.CallLegs
            .Where(l => l.CallSessionId == session.Id
                && l.Kind == CallLegKind.AiWorker && l.EndedAt == null)
            .OrderByDescending(l => l.LegIndex)
            .FirstOrDefaultAsync();
        if (leg != null && leg.AnsweredAt == null)
        {
            leg.AnsweredAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }
    }

    // ── session/leg helpers ───────────────────────────────────────────────

    private async Task EndSessionAsync(CallSession session, string hangupCause)
    {
        session.EndedAt = DateTime.UtcNow;
        session.DurationSeconds = (int)(session.EndedAt.Value - session.StartedAt).TotalSeconds;
        session.Status = session.Status == CallSessionStatus.Transferred
            ? CallSessionStatus.Completed
            : CallSessionStatus.Completed;

        var openTransfers = await _db.CallTransfers
            .Where(t => t.CallSessionId == session.Id
                && (t.Status == CallTransferStatus.Requested
                    || t.Status == CallTransferStatus.Ringing))
            .ToListAsync();
        foreach (var t in openTransfers)
        {
            t.Status = CallTransferStatus.Cancelled;
            t.FailureReason = $"Call ended ({hangupCause})";
            t.UpdatedAt = DateTime.UtcNow;
        }

        var inCallAgents = await _db.CallParticipants
            .Where(p => p.CallSessionId == session.Id
                && p.ParticipantType == ParticipantType.HumanAgent
                && p.LeftAt == null)
            .Select(p => p.HumanAgentId)
            .Distinct()
            .ToListAsync();
        foreach (var agentId in inCallAgents)
        {
            var agent = await _db.HumanAgents.FirstOrDefaultAsync(a => a.Id == agentId!.Value);
            if (agent != null && agent.Status == HumanAgentStatus.InCall)
                agent.Status = HumanAgentStatus.Available;
        }

        var openLegs = await _db.CallLegs
            .Where(l => l.CallSessionId == session.Id && l.EndedAt == null)
            .ToListAsync();
        foreach (var leg in openLegs)
            leg.EndedAt = session.EndedAt;

        await _db.SaveChangesAsync();
    }

    private async Task OpenLegAsync(Guid sessionId, CallLegKind kind, string identity, bool answered = false)
    {
        var exists = await _db.CallLegs.AnyAsync(l =>
            l.CallSessionId == sessionId && l.ParticipantIdentity == identity && l.EndedAt == null);
        if (exists) return;

        var maxIndex = await _db.CallLegs
            .Where(l => l.CallSessionId == sessionId)
            .Select(l => (int?)l.LegIndex)
            .MaxAsync() ?? -1;

        _db.CallLegs.Add(new CallLeg
        {
            CallSessionId = sessionId,
            LegIndex = maxIndex + 1,
            Kind = kind,
            ParticipantIdentity = identity,
            AnsweredAt = answered ? DateTime.UtcNow : null
        });
        await _db.SaveChangesAsync();
    }

    private async Task CloseLegAsync(Guid sessionId, string identity)
    {
        var leg = await _db.CallLegs
            .Where(l => l.CallSessionId == sessionId
                && l.ParticipantIdentity == identity && l.EndedAt == null)
            .OrderByDescending(l => l.LegIndex)
            .FirstOrDefaultAsync();
        if (leg != null)
        {
            leg.EndedAt = DateTime.UtcNow;
            leg.HangupCause = "participant_left";
            await _db.SaveChangesAsync();
        }
    }

    private async Task CloseLegByIdentityAsync(Guid sessionId, string identity, string cause)
    {
        var leg = await _db.CallLegs
            .Where(l => l.CallSessionId == sessionId
                && l.ParticipantIdentity == identity && l.EndedAt == null)
            .OrderByDescending(l => l.LegIndex)
            .FirstOrDefaultAsync();
        if (leg != null)
        {
            leg.EndedAt = DateTime.UtcNow;
            leg.HangupCause = cause;
            await _db.SaveChangesAsync();
        }
    }

    public static Guid? TryGetOwner(string roomName)
    {
        if (!roomName.StartsWith(RoomOwnerPrefix, StringComparison.Ordinal))
            return null;
        var guidPart = roomName[RoomOwnerPrefix.Length..];
        if (guidPart.Length < 36) return null;
        return Guid.TryParse(guidPart[..36], out var g) ? g : (Guid?)null;
    }

    /// <summary>
    /// The participant that gets removed when this transfer completes.
    /// Defaults to the AI worker for legacy/AI-originated transfers; agent- or
    /// destination-originated transfers carry their outgoing party explicitly.
    /// </summary>
    public static string ExtractFromIdentity(string? targetSnapshotJson)
    {
        try
        {
            if (!string.IsNullOrEmpty(targetSnapshotJson))
            {
                using var doc = JsonDocument.Parse(targetSnapshotJson);
                if (doc.RootElement.TryGetProperty("fromIdentity", out var el))
                    return el.GetString() ?? AiIdentity;
            }
        }
        catch
        {
            // Malformed snapshot: fall back to AI removal.
        }
        return AiIdentity;
    }
}
