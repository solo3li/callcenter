namespace backend.Dtos
{
    public sealed record CallTransferDto(
        Guid Id,
        Guid CallSessionId,
        Guid? FromParticipantId,
        Guid ToHumanAgentId,
        string ToHumanAgentName,
        string Status,
        string? Reason,
        string? FailureReason,
        DateTime RequestedAt,
        DateTime? AcceptedAt,
        DateTime? CompletedAt,
        DateTime? FailedAt,
        DateTime CreatedAt,
        DateTime UpdatedAt
    );

    public sealed record InitiateTransferRequest(
        string? Reason = null
    );

    public sealed record TransferResponse(
        CallTransferDto Transfer,
        CallHandoffInfoDto? Handoff
    );

    public sealed record CallHandoffInfoDto(
        Guid Id,
        Guid CallTransferId,
        Guid ToHumanAgentId,
        string ToHumanAgentName,
        string Status,
        string? Summary,
        string? ContextDataJson,
        DateTime CreatedAt
    );
}