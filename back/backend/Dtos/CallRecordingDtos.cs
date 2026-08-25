namespace backend.Dtos
{
    public sealed record CallRecordingDto(
        Guid Id,
        Guid CallSessionId,
        string StorageProvider,
        string ObjectKey,
        string? ContentType,
        int? DurationSeconds,
        long? SizeBytes,
        string Status,
        DateTime CreatedAt,
        DateTime? CompletedAt
    );

    public sealed record RecordingCallbackRequest(
        string ObjectKey,
        string? ContentType = null,
        int? DurationSeconds = null,
        long? SizeBytes = null,
        string Status = "Completed"
    );

    public sealed record DownloadUrlResponse(
        string Url,
        DateTime ExpiresAt
    );
}