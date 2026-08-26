using backend.Models.Enums;

namespace backend.Dtos
{
    public sealed record CallSessionListItem(
        Guid Id,
        Guid UserId,
        Guid? CallConfigurationId,
        string LivekitRoomName,
        string Status,
        string Direction,
        DateTime StartedAt,
        DateTime? AnsweredAt,
        DateTime? EndedAt,
        int? DurationSeconds,
        string? MetadataJson,
        int ParticipantCount,
        DateTime CreatedAt
    );

    public sealed record CallSessionDetail(
        Guid Id,
        Guid UserId,
        Guid? CallConfigurationId,
        string? CallConfigName,
        Guid? PersonaVersionId,
        Guid? WorkflowVersionId,
        Guid? ApiKeyId,
        string LivekitRoomName,
        string? LivekitRoomSid,
        string Status,
        string Direction,
        DateTime StartedAt,
        DateTime? AnsweredAt,
        DateTime? EndedAt,
        int? DurationSeconds,
        string? MetadataJson,
        DateTime CreatedAt,
        List<CallParticipantDto> Participants,
        List<CallTransferDetailDto> Transfers,
        List<CallRecordingDetailDto> Recordings,
        CallHandoffDetailDto? Handoff
    );

    public sealed record ActiveCallDto(
        Guid Id,
        string LivekitRoomName,
        string Status,
        string Direction,
        DateTime StartedAt,
        DateTime? AnsweredAt,
        int? DurationSeconds,
        int ParticipantCount,
        DateTime CreatedAt
    );

    public sealed record CreateCallRequest(
        Guid? CallConfigId = null,
        Guid? PersonaVersionId = null,
        string? RoomName = null,
        string Direction = "Inbound"
    );

    public sealed record EndCallResponse(
        Guid Id,
        string Status,
        int? DurationSeconds,
        DateTime? EndedAt
    );

    public sealed record UpdateMetadataRequest(
        string MetadataJson
    );

    public sealed record CallParticipantDto(
        Guid Id,
        Guid? HumanAgentId,
        string ParticipantType,
        string LivekitIdentity,
        string? LivekitParticipantSid,
        string? DisplayName,
        DateTime JoinedAt,
        DateTime? LeftAt,
        DateTime CreatedAt
    );

    public sealed record CallTransferDetailDto(
        Guid Id,
        Guid CallSessionId,
        Guid? FromParticipantId,
        Guid? ToHumanAgentId,
        string ToHumanAgentName,
        string Status,
        string? Reason,
        string? FailureReason,
        DateTime RequestedAt,
        DateTime? AcceptedAt,
        DateTime? CompletedAt,
        DateTime? FailedAt
    );

    public sealed record CallRecordingDetailDto(
        Guid Id,
        string StorageProvider,
        string ObjectKey,
        string? ContentType,
        int? DurationSeconds,
        long? SizeBytes,
        string Status,
        DateTime CreatedAt,
        DateTime? CompletedAt
    );

    public sealed record CallHandoffDetailDto(
        Guid Id,
        Guid CallTransferId,
        Guid? FromParticipantId,
        Guid ToHumanAgentId,
        string ToHumanAgentName,
        string? Reason,
        string? Summary,
        string? ContextDataJson,
        string Status,
        DateTime CreatedAt,
        DateTime? DeliveredAt,
        DateTime? AcceptedAt
    );
}
