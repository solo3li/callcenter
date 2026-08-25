using System;
using System.Collections.Generic;

namespace backend.Models.Domain;

public class Partner
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string OrganizationName { get; set; } = string.Empty;
    public string? ContactEmail { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Website { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public string? MetadataJson { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
    public ICollection<PartnerRelationship> CustomerRelationships { get; set; } = new List<PartnerRelationship>();
    public ICollection<PartnerPlan> PartnerPlans { get; set; } = new List<PartnerPlan>();
    public ICollection<License> Licenses { get; set; } = new List<License>();
    public ICollection<UsageRecord> UsageRecords { get; set; } = new List<UsageRecord>();
}