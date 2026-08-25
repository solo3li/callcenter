using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Dtos;
using backend.Hubs;
using backend.Models.Domain;
using backend.Models.Enums;

namespace backend.Services
{
    public class CallTransferService
    {
        private readonly AppDbContext _db;
        private readonly IHubContext<CallHub> _hub;

        public CallTransferService(AppDbContext db, IHubContext<CallHub> hub)
        {
            _db = db;
            _hub = hub;
        }

        public async Task<TransferResponse?> InitiateTransferAsync(
            Guid callSessionId,
            Guid userId,
            string? reason)
        {
            var session = await _db.CallSessions
                .FirstOrDefaultAsync(c => c.Id == callSessionId && c.UserId == userId);

            if (session == null)
                return null;

            var fromParticipant = await _db.CallParticipants
                .FirstOrDefaultAsync(p => p.CallSessionId == callSessionId
                    && p.ParticipantType == ParticipantType.AiAgent);

            var availableAgent = await FindAvailableAgentAsync(userId);
            if (availableAgent == null)
                throw new InvalidOperationException("No human agents available");

            var transfer = new CallTransfer
            {
                Id = Guid.NewGuid(),
                CallSessionId = callSessionId,
                FromParticipantId = fromParticipant?.Id,
                ToHumanAgentId = availableAgent.Id,
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

            await _hub.Clients.All.SendAsync("IncomingTransfer", new
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

            if (session != null && session.Status == CallSessionStatus.Queued)
            {
                session.Status = CallSessionStatus.Transferred;
                session.AnsweredAt = DateTime.UtcNow;
            }

            await _db.SaveChangesAsync();

            return new CallTransferDto(
                transfer.Id,
                transfer.CallSessionId,
                transfer.FromParticipantId,
                transfer.ToHumanAgentId,
                transfer.ToHumanAgent.Name,
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

            await _hub.Clients.All.SendAsync("IncomingTransfer", new
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

            await _db.SaveChangesAsync();

            return new CallTransferDto(
                transfer.Id,
                transfer.CallSessionId,
                transfer.FromParticipantId,
                transfer.ToHumanAgentId,
                transfer.ToHumanAgent.Name,
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
                .Select(t => new CallTransferDto(
                    t.Id,
                    t.CallSessionId,
                    t.FromParticipantId,
                    t.ToHumanAgentId,
                    t.ToHumanAgent.Name,
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
                .OrderByDescending(t => t.RequestedAt)
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
                    t.ToHumanAgent.Name,
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

        private async Task<HumanAgent?> FindAvailableAgentAsync(Guid ownerUserId, Guid? excludeAgentId = null)
        {
            var busyStatuses = new List<CallTransferStatus> { CallTransferStatus.Requested, CallTransferStatus.Ringing, CallTransferStatus.Accepted };

            var agents = await _db.HumanAgents
                .Where(a => a.OwnerUserId == ownerUserId
                    && a.Status == HumanAgentStatus.Available
                    && a.IsActive)
                .ToListAsync();

            foreach (var agent in agents)
            {
                if (excludeAgentId.HasValue && agent.Id == excludeAgentId.Value)
                    continue;

                var activeTransfers = await _db.CallTransfers
                    .CountAsync(t => t.ToHumanAgentId == agent.Id && busyStatuses.Contains(t.Status));

                if (activeTransfers < agent.MaxConcurrentCalls)
                    return agent;
            }

            return null;
        }
    }
}