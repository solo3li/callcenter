using System;
using backend.Models.Enums;

namespace backend.Models.Domain;

public class License
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public Guid? PartnerId { get; set; }
    public Guid? PartnerPlanId { get; set; }
    public LicenseStatus Status { get; set; } = LicenseStatus.Active;
    public DateTime StartsAt { get; set; } = DateTime.UtcNow;
    public DateTime? EndsAt { get; set; }
    public string? LimitsJson { get; set; }
    public string? MetadataJson { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
    public Partner? Partner { get; set; }
    public PartnerPlan? PartnerPlan { get; set; }
    public ICollection<UsageRecord> UsageRecords { get; set; } = new List<UsageRecord>();
}