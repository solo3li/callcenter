using System;
using backend.Models.Enums;

namespace backend.Models.Domain;

public class CallParticipant
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CallSessionId { get; set; }
    public Guid? HumanAgentId { get; set; }
    public ParticipantType ParticipantType { get; set; }
    public string LivekitIdentity { get; set; } = string.Empty;
    public string? LivekitParticipantSid { get; set; }
    public string? DisplayName { get; set; }
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LeftAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public CallSession CallSession { get; set; } = null!;
    public HumanAgent? HumanAgent { get; set; }
}