using System;

namespace backend.Models.Domain;

public class HumanAgentSession
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid HumanAgentId { get; set; }
    public string LivekitIdentity { get; set; } = string.Empty;
    public string Status { get; set; } = "active";
    public DateTime ConnectedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DisconnectedAt { get; set; }
    public DateTime? LastHeartbeatAt { get; set; }
    public string? MetadataJson { get; set; }

    public HumanAgent HumanAgent { get; set; } = null!;
}