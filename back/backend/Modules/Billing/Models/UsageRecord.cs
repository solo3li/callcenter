using System;
using backend.Models.Enums;

namespace backend.Modules.Billing.Models;

public class UsageRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public Guid? PartnerId { get; set; }
    public Guid? LicenseId { get; set; }
    public Guid? CallSessionId { get; set; }
    public string IdempotencyKey { get; set; } = string.Empty;
    public MetricType MetricType { get; set; }
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = "seconds";
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    public string? MetadataJson { get; set; }

    public User User { get; set; } = null!;
    public Partner? Partner { get; set; }
    public License? License { get; set; }
    public CallSession? CallSession { get; set; }
}
