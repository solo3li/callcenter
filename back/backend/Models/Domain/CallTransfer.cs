using System;
using backend.Models.Enums;

namespace backend.Models.Domain;

public class CallTransfer
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CallSessionId { get; set; }
    public Guid? FromParticipantId { get; set; }
    public Guid ToHumanAgentId { get; set; }
    public CallTransferStatus Status { get; set; } = CallTransferStatus.Requested;
    public string? Reason { get; set; }
    public string? FailureReason { get; set; }
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public DateTime? AcceptedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? FailedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public CallSession CallSession { get; set; } = null!;
    public CallParticipant? FromParticipant { get; set; }
    public HumanAgent ToHumanAgent { get; set; } = null!;
    public CallHandoff? Handoff { get; set; }
}