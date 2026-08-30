using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Modules.Identity.Dtos;
using backend.Modules.Billing.Dtos;
using backend.Modules.CallOperations.Dtos;
using backend.Modules.Configuration.Dtos;
using backend.Modules.Analytics.Dtos;
using backend.Models.Domain;
using backend.Models.Enums;

namespace backend.Services
{
    public class CallRecordingService
    {
        private readonly AppDbContext _db;
        private readonly StorageService _storage;

        public CallRecordingService(AppDbContext db, StorageService storage)
        {
            _db = db;
            _storage = storage;
        }

        public async Task<List<CallRecordingDto>> ListForCallAsync(Guid callSessionId)
        {
            return await _db.CallRecordings
                .Where(r => r.CallSessionId == callSessionId)
                .Select(r => new CallRecordingDto(
                    r.Id,
                    r.CallSessionId,
                    r.StorageProvider,
                    r.ObjectKey,
                    r.ContentType,
                    r.DurationSeconds,
                    r.SizeBytes,
                    r.Status.ToString(),
                    r.CreatedAt,
                    r.CompletedAt
                ))
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<CallRecordingDto?> GetByIdAsync(Guid recordingId)
        {
            return await _db.CallRecordings
                .Where(r => r.Id == recordingId)
                .Select(r => new CallRecordingDto(
                    r.Id,
                    r.CallSessionId,
                    r.StorageProvider,
                    r.ObjectKey,
                    r.ContentType,
                    r.DurationSeconds,
                    r.SizeBytes,
                    r.Status.ToString(),
                    r.CreatedAt,
                    r.CompletedAt
                ))
                .FirstOrDefaultAsync();
        }

        public async Task<CallRecordingDto> HandleEgressCallback(Guid callSessionId, RecordingCallbackRequest request)
        {
            var status = Enum.TryParse<RecordingStatus>(request.Status, true, out var s)
                ? s
                : RecordingStatus.Completed;

            var recording = new CallRecording
            {
                Id = Guid.NewGuid(),
                CallSessionId = callSessionId,
                StorageProvider = "s3",
                ObjectKey = request.ObjectKey,
                ContentType = request.ContentType,
                DurationSeconds = request.DurationSeconds,
                SizeBytes = request.SizeBytes,
                Status = status,
                CreatedAt = DateTime.UtcNow,
                CompletedAt = status == RecordingStatus.Completed ? DateTime.UtcNow : null
            };

            _db.CallRecordings.Add(recording);
            await _db.SaveChangesAsync();

            return new CallRecordingDto(
                recording.Id,
                recording.CallSessionId,
                recording.StorageProvider,
                recording.ObjectKey,
                recording.ContentType,
                recording.DurationSeconds,
                recording.SizeBytes,
                recording.Status.ToString(),
                recording.CreatedAt,
                recording.CompletedAt
            );
        }

        public async Task<DownloadUrlResponse> GenerateDownloadUrl(Guid recordingId)
        {
            var recording = await _db.CallRecordings
                .FirstOrDefaultAsync(r => r.Id == recordingId);

            if (recording == null)
                return new DownloadUrlResponse(string.Empty, DateTime.UtcNow.AddHours(1));

            var expiresAt = DateTime.UtcNow.AddHours(1);
            var url = await _storage.GeneratePresignedUrlAsync(recording.ObjectKey, 3600);

            return new DownloadUrlResponse(url, expiresAt);
        }

        public async Task<CallRecordingDto> CreateRecordingAsync(
            Guid callSessionId,
            string objectKey,
            string? contentType = null,
            int? durationSeconds = null,
            long? sizeBytes = null)
        {
            var recording = new CallRecording
            {
                Id = Guid.NewGuid(),
                CallSessionId = callSessionId,
                StorageProvider = "s3",
                ObjectKey = objectKey,
                ContentType = contentType,
                DurationSeconds = durationSeconds,
                SizeBytes = sizeBytes,
                Status = RecordingStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            _db.CallRecordings.Add(recording);
            await _db.SaveChangesAsync();

            return new CallRecordingDto(
                recording.Id,
                recording.CallSessionId,
                recording.StorageProvider,
                recording.ObjectKey,
                recording.ContentType,
                recording.DurationSeconds,
                recording.SizeBytes,
                recording.Status.ToString(),
                recording.CreatedAt,
                recording.CompletedAt
            );
        }

        public async Task<bool> DeleteAsync(Guid recordingId)
        {
            var recording = await _db.CallRecordings
                .FirstOrDefaultAsync(r => r.Id == recordingId);

            if (recording == null)
                return false;

            _db.CallRecordings.Remove(recording);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}