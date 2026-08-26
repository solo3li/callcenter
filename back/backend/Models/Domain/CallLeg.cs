using System;
using backend.Models.Enums;

namespace backend.Models.Domain;

/// <summary>
/// One media participant side of a call session. A session has ordered legs;
/// transfers add legs instead of moving the caller.
/// </summary>
public class CallLeg
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CallSessionId { get; set; }
    public int LegIndex { get; set; }
    public CallLegKind Kind { get; set; }
    public string? ParticipantIdentity { get; set; }
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? AnsweredAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public string? HangupCause { get; set; }

    public CallSession CallSession { get; set; } = null!;
}
