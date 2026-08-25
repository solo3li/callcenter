using System;
using backend.Models.Enums;

namespace backend.Models.Domain;

public class PartnerRelationship
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PartnerId { get; set; }
    public Guid CustomerUserId { get; set; }
    public PartnerRelationshipStatus Status { get; set; } = PartnerRelationshipStatus.Active;
    public string? MetadataJson { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Partner Partner { get; set; } = null!;
    public User CustomerUser { get; set; } = null!;
}