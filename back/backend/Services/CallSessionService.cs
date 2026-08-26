using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Dtos;
using backend.Models.Domain;
using backend.Models.Enums;

namespace backend.Services
{
    public class CallSessionService
    {
        private readonly AppDbContext _db;

        public CallSessionService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<(List<CallSessionListItem> Items, int TotalCount)> ListAsync(
            Guid userId,
            string? status,
            string? direction,
            DateTime? from,
            DateTime? to,
            int page,
            int limit)
        {
            var query = _db.CallSessions
                .Where(c => c.UserId == userId)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status) && Enum.TryParse<CallSessionStatus>(status, true, out var statusEnum))
                query = query.Where(c => c.Status == statusEnum);

            if (!string.IsNullOrEmpty(direction) && Enum.TryParse<CallDirection>(direction, true, out var dirEnum))
                query = query.Where(c => c.Direction == dirEnum);

            if (from.HasValue)
                query = query.Where(c => c.StartedAt >= from.Value);

            if (to.HasValue)
                query = query.Where(c => c.StartedAt <= to.Value);

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(c => c.StartedAt)
                .Skip((page - 1) * limit)
                .Take(limit)
                .Select(c => new CallSessionListItem(
                    c.Id,
                    c.UserId,
                    c.CallConfigurationId,
                    c.LivekitRoomName,
                    c.Status.ToString(),
                    c.Direction.ToString(),
                    c.StartedAt,
                    c.AnsweredAt,
                    c.EndedAt,
                    c.DurationSeconds,
                    c.MetadataJson,
                    c.Participants.Count,
                    c.CreatedAt
                ))
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<CallSessionDetail?> GetByIdAsync(Guid id, Guid userId)
        {
            var session = await _db.CallSessions
                .Where(c => c.Id == id && c.UserId == userId)
                .Include(c => c.Participants)
                .Include(c => c.Transfers).ThenInclude(t => t.ToHumanAgent)
                .Include(c => c.Recordings)
                .Include(c => c.Handoffs).ThenInclude(h => h.ToHumanAgent)
                .Include(c => c.CallConfiguration)
                .FirstOrDefaultAsync();

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
                    h.ToHumanAgent.Name,
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

        public async Task<List<ActiveCallDto>> GetActiveAsync(Guid userId)
        {
            var activeStatuses = new List<CallSessionStatus>
            {
                CallSessionStatus.Queued,
                CallSessionStatus.Ringing,
                CallSessionStatus.Active,
                CallSessionStatus.Transferred
            };

            return await _db.CallSessions
                .Where(c => c.UserId == userId && activeStatuses.Contains(c.Status))
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
                .ToListAsync();
        }

        public async Task<CallSession> CreateAsync(
            Guid userId,
            Guid? callConfigId,
            Guid? personaVersionId,
            string roomName,
            string direction)
        {
            var parsedDirection = Enum.TryParse<CallDirection>(direction, true, out var d)
                ? d
                : CallDirection.Inbound;

            var session = new CallSession
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                CallConfigurationId = callConfigId,
                PersonaVersionId = personaVersionId,
                LivekitRoomName = roomName,
                Status = CallSessionStatus.Queued,
                Direction = parsedDirection,
                StartedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            _db.CallSessions.Add(session);
            await _db.SaveChangesAsync();

            return session;
        }

        public async Task<EndCallResponse?> EndCallAsync(Guid id, Guid userId)
        {
            var session = await _db.CallSessions
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (session == null)
                return null;

            var now = DateTime.UtcNow;
            session.EndedAt = now;
            session.DurationSeconds = (int)(now - session.StartedAt).TotalSeconds;
            session.Status = CallSessionStatus.Completed;
            await _db.SaveChangesAsync();

            return new EndCallResponse(
                session.Id,
                session.Status.ToString(),
                session.DurationSeconds,
                session.EndedAt
            );
        }

        public async Task<bool> UpdateMetadataAsync(Guid id, Guid userId, string metadataJson)
        {
            var session = await _db.CallSessions
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (session == null)
                return false;

            session.MetadataJson = metadataJson;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<List<CallParticipantDto>> GetParticipantsAsync(Guid callSessionId, Guid userId)
        {
            var owned = await _db.CallSessions
                .AnyAsync(c => c.Id == callSessionId && c.UserId == userId);

            if (!owned)
                return new List<CallParticipantDto>();

            return await _db.CallParticipants
                .Where(p => p.CallSessionId == callSessionId)
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
                .ToListAsync();
        }

        public async Task<CallParticipantDto?> GetParticipantByIdAsync(Guid callSessionId, Guid participantId, Guid userId)
        {
            var owned = await _db.CallSessions
                .AnyAsync(c => c.Id == callSessionId && c.UserId == userId);

            if (!owned)
                return null;

            return await _db.CallParticipants
                .Where(p => p.CallSessionId == callSessionId && p.Id == participantId)
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
                .FirstOrDefaultAsync();
        }

        public static string GenerateRoomName()
        {
            var bytes = RandomNumberGenerator.GetBytes(4);
            var hex = new StringBuilder(8);
            foreach (var b in bytes)
                hex.Append(b.ToString("x2"));
            return $"call_{hex}";
        }
    }
}