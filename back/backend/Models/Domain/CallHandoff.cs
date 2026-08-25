using System;
using backend.Models.Enums;

namespace backend.Models.Domain;

public class CallHandoff
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CallSessionId { get; set; }
    public Guid CallTransferId { get; set; }
    public Guid? FromParticipantId { get; set; }
    public Guid ToHumanAgentId { get; set; }
    public string? Reason { get; set; }
    public string? Summary { get; set; }
    public string? ContextDataJson { get; set; }
    public HandoffStatus Status { get; set; } = HandoffStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeliveredAt { get; set; }
    public DateTime? AcceptedAt { get; set; }

    public CallSession CallSession { get; set; } = null!;
    public CallTransfer CallTransfer { get; set; } = null!;
    public CallParticipant? FromParticipant { get; set; }
    public HumanAgent ToHumanAgent { get; set; } = null!;
}