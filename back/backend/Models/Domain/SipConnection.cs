using System;

namespace backend.Models.Domain;

/// <summary>
/// An inbound SIP trunk delivered by the customer PBX into LiveKit-SIP.
/// One row per customer connection; ownership resolves inbound calls to a user.
/// </summary>
public class SipConnection
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string[] AllowedIps { get; set; } = Array.Empty<string>();
    public string[] Numbers { get; set; } = Array.Empty<string>();
    public string? LkTrunkId { get; set; }
    public string? DispatchRuleId { get; set; }
    public bool IsActive { get; set; } = true;
    public int MaxConcurrentCalls { get; set; } = 10;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
}
