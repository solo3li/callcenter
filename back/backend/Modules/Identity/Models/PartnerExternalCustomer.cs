using System;

namespace backend.Modules.Identity.Models;

public class PartnerExternalCustomer
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PartnerId { get; set; }
    public string ExternalCustomerId { get; set; } = string.Empty;
    public Guid PlatformUserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Partner Partner { get; set; } = null!;
    public User PlatformUser { get; set; } = null!;
}
