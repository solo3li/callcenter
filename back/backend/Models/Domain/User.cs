using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using backend.Models.Enums;

namespace backend.Models.Domain;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? CompanyName { get; set; }

    [JsonIgnore]
    public string PasswordHash { get; set; } = string.Empty;

    public UserStatus Status { get; set; } = UserStatus.Active;

    public bool IsPartner { get; set; } = false;

    public decimal StandardCredits { get; set; } = 0;
    public decimal PremiumCredits { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Partner? Partner { get; set; }

    public ICollection<ApiKey> ApiKeys { get; set; } = new List<ApiKey>();
    public ICollection<Persona> Personas { get; set; } = new List<Persona>();
    public ICollection<Workflow> Workflows { get; set; } = new List<Workflow>();
    public ICollection<CallConfiguration> CallConfigurations { get; set; } = new List<CallConfiguration>();
    public ICollection<HumanAgent> HumanAgents { get; set; } = new List<HumanAgent>();
    public ICollection<CallSession> CallSessions { get; set; } = new List<CallSession>();
    public ICollection<UsageRecord> UsageRecords { get; set; } = new List<UsageRecord>();
    public ICollection<License> Licenses { get; set; } = new List<License>();
    public ICollection<KnowledgeBase> KnowledgeBases { get; set; } = new List<KnowledgeBase>();
    public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();

    public ICollection<PartnerRelationship> PartnerRelationships { get; set; } = new List<PartnerRelationship>();
}