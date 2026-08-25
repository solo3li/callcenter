using System;
using backend.Models.Enums;

namespace backend.Models.Domain;

public class CallRecording
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CallSessionId { get; set; }
    public string StorageProvider { get; set; } = "s3";
    public string ObjectKey { get; set; } = string.Empty;
    public string? ContentType { get; set; }
    public int? DurationSeconds { get; set; }
    public long? SizeBytes { get; set; }
    public RecordingStatus Status { get; set; } = RecordingStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }

    public CallSession CallSession { get; set; } = null!;
}