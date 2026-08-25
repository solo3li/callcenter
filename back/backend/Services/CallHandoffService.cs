using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Dtos;
using backend.Models.Domain;
using backend.Models.Enums;

namespace backend.Services
{
    public class CallHandoffService
    {
        private readonly AppDbContext _db;

        public CallHandoffService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<CallHandoffDto?> CreateContextAsync(
            Guid transferId,
            string summary,
            string? contextData,
            string? reason)
        {
            var handoff = await _db.CallHandoffs
                .Include(h => h.ToHumanAgent)
                .FirstOrDefaultAsync(h => h.CallTransferId == transferId);

            if (handoff == null)
                return null;

            handoff.Summary = summary;
            handoff.ContextDataJson = contextData;
            handoff.Reason = reason ?? handoff.Reason;
            await _db.SaveChangesAsync();

            return new CallHandoffDto(
                handoff.Id,
                handoff.CallSessionId,
                handoff.CallTransferId,
                handoff.FromParticipantId,
                handoff.ToHumanAgentId,
                handoff.ToHumanAgent.Name,
                handoff.Reason,
                handoff.Summary,
                handoff.ContextDataJson,
                handoff.Status.ToString(),
                handoff.CreatedAt,
                handoff.DeliveredAt,
                handoff.AcceptedAt
            );
        }

        public async Task<CallHandoffDto?> DeliverAsync(Guid handoffId)
        {
            var handoff = await _db.CallHandoffs
                .Include(h => h.ToHumanAgent)
                .FirstOrDefaultAsync(h => h.Id == handoffId);

            if (handoff == null)
                return null;

            handoff.DeliveredAt = DateTime.UtcNow;
            handoff.Status = HandoffStatus.Delivered;
            await _db.SaveChangesAsync();

            return new CallHandoffDto(
                handoff.Id,
                handoff.CallSessionId,
                handoff.CallTransferId,
                handoff.FromParticipantId,
                handoff.ToHumanAgentId,
                handoff.ToHumanAgent.Name,
                handoff.Reason,
                handoff.Summary,
                handoff.ContextDataJson,
                handoff.Status.ToString(),
                handoff.CreatedAt,
                handoff.DeliveredAt,
                handoff.AcceptedAt
            );
        }

        public async Task<CallHandoffDto?> AcceptAsync(Guid handoffId)
        {
            var handoff = await _db.CallHandoffs
                .Include(h => h.ToHumanAgent)
                .FirstOrDefaultAsync(h => h.Id == handoffId);

            if (handoff == null)
                return null;

            handoff.AcceptedAt = DateTime.UtcNow;
            handoff.Status = HandoffStatus.Accepted;
            await _db.SaveChangesAsync();

            return new CallHandoffDto(
                handoff.Id,
                handoff.CallSessionId,
                handoff.CallTransferId,
                handoff.FromParticipantId,
                handoff.ToHumanAgentId,
                handoff.ToHumanAgent.Name,
                handoff.Reason,
                handoff.Summary,
                handoff.ContextDataJson,
                handoff.Status.ToString(),
                handoff.CreatedAt,
                handoff.DeliveredAt,
                handoff.AcceptedAt
            );
        }

        public async Task<List<CallHandoffDto>> ListForCallAsync(Guid callSessionId)
        {
            return await _db.CallHandoffs
                .Where(h => h.CallSessionId == callSessionId)
                .Include(h => h.ToHumanAgent)
                .Select(h => new CallHandoffDto(
                    h.Id,
                    h.CallSessionId,
                    h.CallTransferId,
                    h.FromParticipantId,
                    h.ToHumanAgentId,
                    h.ToHumanAgent.Name,
                    h.Reason,
                    h.Summary,
                    h.ContextDataJson,
                    h.Status.ToString(),
                    h.CreatedAt,
                    h.DeliveredAt,
                    h.AcceptedAt
                ))
                .OrderByDescending(h => h.CreatedAt)
                .ToListAsync();
        }

        public async Task<CallHandoffDto?> GetByIdAsync(Guid handoffId)
        {
            return await _db.CallHandoffs
                .Where(h => h.Id == handoffId)
                .Include(h => h.ToHumanAgent)
                .Select(h => new CallHandoffDto(
                    h.Id,
                    h.CallSessionId,
                    h.CallTransferId,
                    h.FromParticipantId,
                    h.ToHumanAgentId,
                    h.ToHumanAgent.Name,
                    h.Reason,
                    h.Summary,
                    h.ContextDataJson,
                    h.Status.ToString(),
                    h.CreatedAt,
                    h.DeliveredAt,
                    h.AcceptedAt
                ))
                .FirstOrDefaultAsync();
        }
    }
}