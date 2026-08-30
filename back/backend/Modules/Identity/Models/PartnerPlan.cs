using System;
using backend.Models.Enums;

namespace backend.Modules.Identity.Models;

public class PartnerPlan
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PartnerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public string? EntitlementsJson { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Partner Partner { get; set; } = null!;
    public ICollection<License> Licenses { get; set; } = new List<License>();
}
