using System;

namespace backend.Models.Domain;

/// <summary>
/// A named logical transfer target on the customer's external PBX
/// (e.g. "Support" -> ring group / queue entry point).
/// The AI layer only ever sees Name; CallTo stays server-side.
/// </summary>
public class SipDestination
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string CallTo { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
}
