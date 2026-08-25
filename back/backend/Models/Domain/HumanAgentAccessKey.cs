using System;
using backend.Models.Enums;

namespace backend.Models.Domain;

public class HumanAgentAccessKey
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid HumanAgentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string KeyPrefix { get; set; } = string.Empty;
    public string KeyHash { get; set; } = string.Empty;
    public AccessKeyStatus Status { get; set; } = AccessKeyStatus.Active;
    public DateTime? ExpiresAt { get; set; }
    public DateTime? LastUsedAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public HumanAgent HumanAgent { get; set; } = null!;
}