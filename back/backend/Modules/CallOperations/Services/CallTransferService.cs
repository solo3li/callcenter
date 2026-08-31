using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Modules.Identity.Dtos;
using backend.Modules.Billing.Dtos;
using backend.Modules.CallOperations.Dtos;
using backend.Modules.Configuration.Dtos;
using backend.Modules.Analytics.Dtos;
using backend.Hubs;
using backend.Models.Domain;
using backend.Models.Enums;

namespace backend.Services
{
    public class CallTransferService
    {
        private readonly AppDbContext _db;
        private readonly IHubContext<CallHub> _hub;
        private readonly LiveKitService _liveKit;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<CallTransferService> _logger;

        public CallTransferService(AppDbContext db, IHubContext<CallHub> hub,
            LiveKitService liveKit, IServiceScopeFactory scopeFactory,
            ILogger<CallTransferService> logger)
        {
            _db = db;
            _hub = hub;
            _liveKit = liveKit;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public async Task<TransferResponse?> InitiateTransferAsync(
            Guid callSessionId,
            Guid userId,
            string? reason,
            Guid? preferredAgentId = null,
            string? preferredAgentName = null)
        {
            var session = await _db.CallSessions
                .FirstOrDefaultAsync(c => c.Id == callSessionId && c.UserId == userId);

            if (session == null)
                return null;

            var fromParticipant = await _db.CallParticipants
                .FirstOrDefaultAsync(p => p.CallSessionId == callSessionId
                    && p.ParticipantType == ParticipantType.AiAgent);

            var availableAgent = await FindAvailableAgentAsync(userId,
                excludeAgentId: null,
                preferredAgentId: preferredAgentId,
                preferredAgentName: preferredAgentName);
            if (availableAgent == null)
                throw new InvalidOperationException(
                    preferredAgentName != null
                        ? $"Agent '{preferredAgentName}' is not available"
                        : "No human agents available");

            var transfer = new CallTransfer
            {
                Id = Guid.NewGuid(),
                CallSessionId = callSessionId,
                FromParticipantId = fromParticipant?.Id,
                ToHumanAgentId = availableAgent.Id,
                Mode = TransferMode.Cold,
                TargetType = TransferTargetType.HumanAgent,
                TargetSnapshotJson = JsonSerializer.Serialize(new
                {
                    fromIdentity = InboundRoutingService.AiIdentity,
                    agentId = availableAgent.Id,
                    name = availableAgent.Name
                }),
                Status = CallTransferStatus.Requested,
                Reason = reason,
                RequestedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _db.CallTransfers.Add(transfer);

            var handoff = new CallHandoff
            {
                Id = Guid.NewGuid(),
                CallSessionId = callSessionId,
                CallTransferId = transfer.Id,
                FromParticipantId = fromParticipant?.Id,
                ToHumanAgentId = availableAgent.Id,
                Status = HandoffStatus.Pending,
                Reason = reason,
                CreatedAt = DateTime.UtcNow
            };

            _db.CallHandoffs.Add(handoff);

            await _db.SaveChangesAsync();

            await _hub.Clients.Group($"agent_{availableAgent.Id}").SendAsync("IncomingTransfer", new
            {
                transferId = transfer.Id,
                handoffId = handoff.Id,
                callSessionId,
                roomName = session.LivekitRoomName,
                toHumanAgentId = availableAgent.Id,
                reason
            });

            return new TransferResponse(
                new CallTransferDto(
                    transfer.Id,
                    transfer.CallSessionId,
                    transfer.FromParticipantId,
                    transfer.ToHumanAgentId,
                    availableAgent.Name,
                    transfer.Status.ToString(),
                    transfer.Reason,
                    transfer.FailureReason,
                    transfer.RequestedAt,
                    transfer.AcceptedAt,
                    transfer.CompletedAt,
                    transfer.FailedAt,
                    transfer.CreatedAt,
                    transfer.UpdatedAt
                ),
                new CallHandoffInfoDto(
                    handoff.Id,
                    handoff.CallTransferId,
                    handoff.ToHumanAgentId,
                    availableAgent.Name,
                    handoff.Status.ToString(),
                    handoff.Summary,
                    handoff.ContextDataJson,
                    handoff.CreatedAt
                )
            );
        }

        public async Task<CallTransferDto?> AcceptTransferAsync(Guid transferId, Guid humanAgentId)
        {
            var transfer = await _db.CallTransfers
                .Include(t => t.ToHumanAgent)
                .FirstOrDefaultAsync(t => t.Id == transferId);

            if (transfer == null || transfer.ToHumanAgentId != humanAgentId)
                return null;

            transfer.Status = CallTransferStatus.Accepted;
            transfer.AcceptedAt = DateTime.UtcNow;
            transfer.UpdatedAt = DateTime.UtcNow;

            var session = await _db.CallSessions
                .FirstOrDefaultAsync(c => c.Id == transfer.CallSessionId);

            if (session != null)
            {
                if (session.Status == CallSessionStatus.Queued)
                {
                    session.Status = CallSessionStatus.Transferred;
                    session.AnsweredAt = DateTime.UtcNow;
                }
                if (session.Status == CallSessionStatus.Active)
                {
                    session.Status = CallSessionStatus.Transferred;
                }
            }

            if (transfer.ToHumanAgent != null)
            {
                transfer.ToHumanAgent.Status = HumanAgentStatus.InCall;
            }

            await _db.SaveChangesAsync();

            // Cold-swap fallback for webhook-less setups: remove the originating
            // party shortly after accept. Webhook-driven swap remains authoritative.
            var roomName = session?.LivekitRoomName;
            if (!string.IsNullOrEmpty(roomName))
            {
                var fromIdentity = InboundRoutingService.ExtractFromIdentity(transfer.TargetSnapshotJson);
                _ = Task.Run(async () =>
                {
                    await Task.Delay(TimeSpan.FromSeconds(3));
                    using var scope = _scopeFactory.CreateScope();
                    var lk = scope.ServiceProvider.GetRequiredService<LiveKitService>();
                    await lk.RemoveParticipant(roomName, fromIdentity);
                });
            }

            return new CallTransferDto(
                transfer.Id,
                transfer.CallSessionId,
                transfer.FromParticipantId,
                transfer.ToHumanAgentId,
                transfer.ToHumanAgent?.Name ?? "Human",
                transfer.Status.ToString(),
                transfer.Reason,
                transfer.FailureReason,
                transfer.RequestedAt,
                transfer.AcceptedAt,
                transfer.CompletedAt,
                transfer.FailedAt,
                transfer.CreatedAt,
                transfer.UpdatedAt
            );
        }

        public async Task<TransferResponse?> RejectTransferAsync(Guid transferId, Guid humanAgentId)
        {
            var transfer = await _db.CallTransfers
                .Include(t => t.ToHumanAgent)
                .FirstOrDefaultAsync(t => t.Id == transferId);

            if (transfer == null || transfer.ToHumanAgentId != humanAgentId)
                return null;

            transfer.Status = CallTransferStatus.Rejected;
            transfer.FailedAt = DateTime.UtcNow;
            transfer.FailureReason = "Rejected by agent";
            transfer.UpdatedAt = DateTime.UtcNow;

            var handoff = await _db.CallHandoffs
                .FirstOrDefaultAsync(h => h.CallTransferId == transferId);

            if (handoff != null)
                handoff.Status = HandoffStatus.Expired;

            await _db.SaveChangesAsync();

            var ownerUser = await _db.HumanAgents
                .Where(a => a.Id == humanAgentId)
                .Select(a => a.OwnerUserId)
                .FirstOrDefaultAsync();

            var nextAgent = await FindAvailableAgentAsync(ownerUser, humanAgentId);
            if (nextAgent == null)
                return null;

            var session = await _db.CallSessions
                .FirstOrDefaultAsync(c => c.Id == transfer.CallSessionId);

            var fromParticipant = await _db.CallParticipants
                .FirstOrDefaultAsync(p => p.CallSessionId == transfer.CallSessionId
                    && p.ParticipantType == ParticipantType.AiAgent);

            var newTransfer = new CallTransfer
            {
                Id = Guid.NewGuid(),
                CallSessionId = transfer.CallSessionId,
                FromParticipantId = fromParticipant?.Id,
                ToHumanAgentId = nextAgent.Id,
                Mode = TransferMode.Cold,
                TargetType = TransferTargetType.HumanAgent,
                TargetSnapshotJson = JsonSerializer.Serialize(new
                {
                    fromIdentity = InboundRoutingService.ExtractFromIdentity(transfer.TargetSnapshotJson),
                    agentId = nextAgent.Id,
                    name = nextAgent.Name
                }),
                Status = CallTransferStatus.Requested,
                Reason = transfer.Reason,
                RequestedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _db.CallTransfers.Add(newTransfer);

            var newHandoff = new CallHandoff
            {
                Id = Guid.NewGuid(),
                CallSessionId = transfer.CallSessionId,
                CallTransferId = newTransfer.Id,
                FromParticipantId = fromParticipant?.Id,
                ToHumanAgentId = nextAgent.Id,
                Status = HandoffStatus.Pending,
                Reason = transfer.Reason,
                CreatedAt = DateTime.UtcNow
            };

            _db.CallHandoffs.Add(newHandoff);
            await _db.SaveChangesAsync();

            await _hub.Clients.Group($"agent_{nextAgent.Id}").SendAsync("IncomingTransfer", new
            {
                transferId = newTransfer.Id,
                handoffId = newHandoff.Id,
                callSessionId = transfer.CallSessionId,
                roomName = session?.LivekitRoomName,
                toHumanAgentId = nextAgent.Id,
                reason = transfer.Reason
            });

            return new TransferResponse(
                new CallTransferDto(
                    newTransfer.Id,
                    newTransfer.CallSessionId,
                    newTransfer.FromParticipantId,
                    newTransfer.ToHumanAgentId,
                    nextAgent.Name,
                    newTransfer.Status.ToString(),
                    newTransfer.Reason,
                    newTransfer.FailureReason,
                    newTransfer.RequestedAt,
                    newTransfer.AcceptedAt,
                    newTransfer.CompletedAt,
                    newTransfer.FailedAt,
                    newTransfer.CreatedAt,
                    newTransfer.UpdatedAt
                ),
                new CallHandoffInfoDto(
                    newHandoff.Id,
                    newHandoff.CallTransferId,
                    newHandoff.ToHumanAgentId,
                    nextAgent.Name,
                    newHandoff.Status.ToString(),
                    newHandoff.Summary,
                    newHandoff.ContextDataJson,
                    newHandoff.CreatedAt
                )
            );
        }

        public async Task<CallTransferDto?> CompleteTransferAsync(Guid transferId)
        {
            var transfer = await _db.CallTransfers
                .Include(t => t.ToHumanAgent)
                .Include(t => t.Destination)
                .FirstOrDefaultAsync(t => t.Id == transferId);

            if (transfer == null)
                return null;

            transfer.Status = CallTransferStatus.Completed;
            transfer.CompletedAt = DateTime.UtcNow;
            transfer.UpdatedAt = DateTime.UtcNow;

            var handoff = await _db.CallHandoffs
                .FirstOrDefaultAsync(h => h.CallTransferId == transferId);

            if (handoff != null)
                handoff.Status = HandoffStatus.Accepted;

            if (transfer.ToHumanAgent != null)
                transfer.ToHumanAgent.Status = HumanAgentStatus.Available;

            await _db.SaveChangesAsync();

            return new CallTransferDto(
                transfer.Id,
                transfer.CallSessionId,
                transfer.FromParticipantId,
                transfer.ToHumanAgentId,
                transfer.ToHumanAgent?.Name ?? transfer.Destination?.Name ?? "External",
                transfer.Status.ToString(),
                transfer.Reason,
                transfer.FailureReason,
                transfer.RequestedAt,
                transfer.AcceptedAt,
                transfer.CompletedAt,
                transfer.FailedAt,
                transfer.CreatedAt,
                transfer.UpdatedAt
            );
        }

        public async Task<List<CallTransferDto>> ListForCallAsync(Guid callSessionId)
        {
            return await _db.CallTransfers
                .Where(t => t.CallSessionId == callSessionId)
                .Include(t => t.ToHumanAgent)
                .OrderByDescending(t => t.RequestedAt)
                .Select(t => new CallTransferDto(
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
                    t.FailedAt,
                    t.CreatedAt,
                    t.UpdatedAt
                ))
                .ToListAsync();
        }

        public async Task<CallTransferDto?> GetByIdAsync(Guid callSessionId, Guid transferId)
        {
            return await _db.CallTransfers
                .Where(t => t.CallSessionId == callSessionId && t.Id == transferId)
                .Include(t => t.ToHumanAgent)
                .Select(t => new CallTransferDto(
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
                    t.FailedAt,
                    t.CreatedAt,
                    t.UpdatedAt
                ))
                .FirstOrDefaultAsync();
        }

        private async Task<HumanAgent?> FindAvailableAgentAsync(Guid ownerUserId,
            Guid? excludeAgentId = null, Guid? preferredAgentId = null,
            string? preferredAgentName = null)
        {
            var busyStatuses = new List<CallTransferStatus> { CallTransferStatus.Requested, CallTransferStatus.Ringing, CallTransferStatus.Accepted };

            var agents = await _db.HumanAgents
                .Where(a => a.OwnerUserId == ownerUserId
                    && a.Status == HumanAgentStatus.Available
                    && a.IsActive)
                .ToListAsync();

            if (preferredAgentId.HasValue || !string.IsNullOrEmpty(preferredAgentName))
            {
                var named = agents
                    .Where(a => (preferredAgentId.HasValue && a.Id == preferredAgentId.Value)
                        || (!string.IsNullOrEmpty(preferredAgentName) &&
                            (string.Equals(a.Name, preferredAgentName, StringComparison.OrdinalIgnoreCase))))
                    .FirstOrDefault();
                if (named != null && await HasCapacityAsync(named, busyStatuses)) return named;

                // Fallback: unique prefix match on the spoken name.
                if (!string.IsNullOrEmpty(preferredAgentName))
                {
                    var fuzzy = agents.FirstOrDefault(a =>
                        a.Name.StartsWith(preferredAgentName, StringComparison.OrdinalIgnoreCase));
                    if (fuzzy != null && await HasCapacityAsync(fuzzy, busyStatuses)) return fuzzy;
                }
                return null; // explicit target requested — never substitute another agent
            }

            foreach (var agent in agents)
            {
                if (excludeAgentId.HasValue && agent.Id == excludeAgentId.Value)
                    continue;

                if (await HasCapacityAsync(agent, busyStatuses))
                    return agent;
            }

            return null;
        }

        private async Task<bool> HasCapacityAsync(HumanAgent agent, List<CallTransferStatus> busyStatuses)
        {
            var activeTransfers = await _db.CallTransfers
                .CountAsync(t => t.ToHumanAgentId == agent.Id && busyStatuses.Contains(t.Status));
            return activeTransfers < agent.MaxConcurrentCalls;
        }

        // ── v0 destination transfers (external PBX via outbound trunk) ────

        public async Task<CallTransferDto?> InitiateDestinationTransferAsync(
            Guid callSessionId, Guid userId, string destinationName, string? reason)
        {
            var session = await _db.CallSessions
                .FirstOrDefaultAsync(c => c.Id == callSessionId && c.UserId == userId);
            if (session == null) return null;

            var destination = await _db.SipDestinations
                .FirstOrDefaultAsync(d => d.UserId == userId
                    && d.IsEnabled
                    && d.Name.ToLower() == destinationName.ToLower());
            if (destination == null)
                throw new InvalidOperationException($"Destination '{destinationName}' not found");

            var fromParticipant = await _db.CallParticipants
                .FirstOrDefaultAsync(p => p.CallSessionId == callSessionId
                    && p.ParticipantType == ParticipantType.AiAgent);

            var transfer = new CallTransfer
            {
                Id = Guid.NewGuid(),
                CallSessionId = callSessionId,
                FromParticipantId = fromParticipant?.Id,
                DestinationId = destination.Id,
                Mode = TransferMode.Cold,
                TargetType = TransferTargetType.ExternalDestination,
                TargetSnapshotJson = JsonSerializer.Serialize(new
                {
                    fromIdentity = InboundRoutingService.AiIdentity,
                    name = destination.Name,
                    callTo = destination.CallTo
                }),
                Status = CallTransferStatus.Requested,
                Reason = reason,
                RequestedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _db.CallTransfers.Add(transfer);
            await _db.SaveChangesAsync();

            // Fire-and-forget dial-out; completion is observed by webhook or poller.
            var transferId = transfer.Id;
            var roomId = session.Id;
            var roomName = session.LivekitRoomName;
            var identity = $"{InboundRoutingService.DestinationIdentityPrefix}{destination.Id}";
            _ = Task.Run(() => DialOutAndSwapAsync(transferId, roomId, roomName,
                destination.CallTo, identity));

            return new CallTransferDto(
                transfer.Id,
                transfer.CallSessionId,
                transfer.FromParticipantId,
                transfer.ToHumanAgentId,
                destination.Name,
                transfer.Status.ToString(),
                transfer.Reason,
                transfer.FailureReason,
                transfer.RequestedAt,
                transfer.AcceptedAt,
                transfer.CompletedAt,
                transfer.FailedAt,
                transfer.CreatedAt,
                transfer.UpdatedAt
            );
        }

        // ── Agent-originated transfers (agent app) ────────────────────────

        private async Task<(CallSession?, HumanAgent?)> ValidateRequestingAgentAsync(
            Guid callSessionId, Guid requesterAgentId)
        {
            var session = await _db.CallSessions
                .FirstOrDefaultAsync(c => c.Id == callSessionId);
            if (session == null ||
                (session.Status != CallSessionStatus.Transferred
                    && session.Status != CallSessionStatus.Active))
                return (null, null);

            var requester = await _db.HumanAgents
                .FirstOrDefaultAsync(a => a.Id == requesterAgentId && a.IsActive);
            if (requester == null || requester.OwnerUserId != session.UserId)
                return (null, null);

            return (session, requester);
        }

        public async Task<CallTransferDto?> InitiateAgentHumanTransferAsync(
            Guid callSessionId, Guid requesterAgentId, string? targetName, string? reason)
        {
            var (session, requester) = await ValidateRequestingAgentAsync(callSessionId, requesterAgentId);
            if (session == null || requester == null) return null;

            var availableAgent = await FindAvailableAgentAsync(
                requester.OwnerUserId,
                excludeAgentId: requesterAgentId,
                preferredAgentName: string.IsNullOrWhiteSpace(targetName) ? null : targetName);
            if (availableAgent == null)
                throw new InvalidOperationException(
                    !string.IsNullOrWhiteSpace(targetName)
                        ? $"Agent '{targetName}' is not available"
                        : "No human agents available");

            var fromIdentity = $"{InboundRoutingService.AgentIdentityPrefix}{requesterAgentId}";
            var transfer = new CallTransfer
            {
                Id = Guid.NewGuid(),
                CallSessionId = callSessionId,
                ToHumanAgentId = availableAgent.Id,
                Mode = TransferMode.Cold,
                TargetType = TransferTargetType.HumanAgent,
                TargetSnapshotJson = JsonSerializer.Serialize(new
                {
                    fromIdentity,
                    agentId = availableAgent.Id,
                    name = availableAgent.Name
                }),
                Status = CallTransferStatus.Requested,
                Reason = reason,
                RequestedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _db.CallTransfers.Add(transfer);

            var handoff = new CallHandoff
            {
                Id = Guid.NewGuid(),
                CallSessionId = callSessionId,
                CallTransferId = transfer.Id,
                ToHumanAgentId = availableAgent.Id,
                Status = HandoffStatus.Pending,
                Reason = reason,
                CreatedAt = DateTime.UtcNow
            };
            _db.CallHandoffs.Add(handoff);
            await _db.SaveChangesAsync();

            await _hub.Clients.Group($"agent_{availableAgent.Id}").SendAsync("IncomingTransfer", new
            {
                transferId = transfer.Id,
                handoffId = handoff.Id,
                callSessionId,
                roomName = session.LivekitRoomName,
                toHumanAgentId = availableAgent.Id,
                reason
            });

            return new CallTransferDto(
                transfer.Id,
                transfer.CallSessionId,
                transfer.FromParticipantId,
                transfer.ToHumanAgentId,
                availableAgent.Name,
                transfer.Status.ToString(),
                transfer.Reason,
                transfer.FailureReason,
                transfer.RequestedAt,
                transfer.AcceptedAt,
                transfer.CompletedAt,
                transfer.FailedAt,
                transfer.CreatedAt,
                transfer.UpdatedAt
            );
        }

        public async Task<CallTransferDto?> InitiateAgentDestinationTransferAsync(
            Guid callSessionId, Guid requesterAgentId, string destinationName, string? reason)
        {
            var (session, requester) = await ValidateRequestingAgentAsync(callSessionId, requesterAgentId);
            if (session == null || requester == null) return null;

            var destination = await _db.SipDestinations
                .FirstOrDefaultAsync(d => d.UserId == session.UserId
                    && d.IsEnabled
                    && d.Name.ToLower() == destinationName.ToLower());
            if (destination == null)
                throw new InvalidOperationException($"Destination '{destinationName}' not found");

            var fromIdentity = $"{InboundRoutingService.AgentIdentityPrefix}{requesterAgentId}";
            var transfer = new CallTransfer
            {
                Id = Guid.NewGuid(),
                CallSessionId = callSessionId,
                DestinationId = destination.Id,
                Mode = TransferMode.Cold,
                TargetType = TransferTargetType.ExternalDestination,
                TargetSnapshotJson = JsonSerializer.Serialize(new
                {
                    fromIdentity,
                    name = destination.Name,
                    callTo = destination.CallTo
                }),
                Status = CallTransferStatus.Requested,
                Reason = reason,
                RequestedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _db.CallTransfers.Add(transfer);
            await _db.SaveChangesAsync();

            var identity = $"{InboundRoutingService.DestinationIdentityPrefix}{destination.Id}";
            _ = Task.Run(() => DialOutAndSwapAsync(transfer.Id, session.Id, session.LivekitRoomName,
                destination.CallTo, identity));

            return new CallTransferDto(
                transfer.Id,
                transfer.CallSessionId,
                transfer.FromParticipantId,
                transfer.ToHumanAgentId,
                destination.Name,
                transfer.Status.ToString(),
                transfer.Reason,
                transfer.FailureReason,
                transfer.RequestedAt,
                transfer.AcceptedAt,
                transfer.CompletedAt,
                transfer.FailedAt,
                transfer.CreatedAt,
                transfer.UpdatedAt
            );
        }

        private async Task DialOutAndSwapAsync(Guid transferId, Guid sessionId,
            string roomName, string callTo, string identity)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var liveKit = scope.ServiceProvider.GetRequiredService<LiveKitService>();
                var routing = scope.ServiceProvider.GetRequiredService<InboundRoutingService>();

                var trunkId = Environment.GetEnvironmentVariable("LIVEKIT_OUTBOUND_TRUNK_ID");
                if (string.IsNullOrEmpty(trunkId))
                {
                    await FailDestinationTransferAsync(db, transferId, "Outbound trunk is not configured");
                    return;
                }

                var created = await liveKit.CreateSipParticipant(trunkId, callTo, roomName, identity,
                    displayName: $"dest-{callTo}");
                if (created == null)
                {
                    await FailDestinationTransferAsync(db, transferId, "SIP dial failed");
                    return;
                }

                // Poll for the bridged participant answering (webhook completes sooner).
                var deadline = DateTime.UtcNow.AddSeconds(45);
                while (DateTime.UtcNow < deadline)
                {
                    var identities = await liveKit.ListParticipantIdentities(roomName);
                    if (identities.Contains(identity))
                        return; // webhook performs the swap

                    await Task.Delay(TimeSpan.FromSeconds(3));

                    var t = await db.CallTransfers.FindAsync(transferId);
                    if (t == null || t.Status != CallTransferStatus.Requested)
                        return; // already completed/failed elsewhere
                }

                await FailDestinationTransferAsync(db, transferId, "Destination did not answer within 45s");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Dial-out failed for transfer {Transfer}", transferId);
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                await FailDestinationTransferAsync(db, transferId, ex.Message);
            }
        }

        private static async Task FailDestinationTransferAsync(AppDbContext db,
            Guid transferId, string reason)
        {
            var transfer = await db.CallTransfers.FindAsync(transferId);
            if (transfer == null || transfer.Status != CallTransferStatus.Requested) return;
            transfer.Status = CallTransferStatus.Failed;
            transfer.FailureReason = reason;
            transfer.FailedAt = DateTime.UtcNow;
            transfer.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
        }
    }
}