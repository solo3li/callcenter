using System;
using backend.Models.Enums;

namespace backend.Modules.Billing.Models;

public class Plan
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public PlanTier Tier { get; set; } = PlanTier.Starter;
    public bool IsPlatformPlan { get; set; } = true;
    public bool IsActive { get; set; } = true;
    public string? EntitlementsJson { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
}
