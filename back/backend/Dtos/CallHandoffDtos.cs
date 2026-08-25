namespace backend.Dtos
{
    public sealed record CallHandoffDto(
        Guid Id,
        Guid CallSessionId,
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

    public sealed record CreateHandoffRequest(
        string Summary,
        string? ContextDataJson = null,
        string? Reason = null
    );
}