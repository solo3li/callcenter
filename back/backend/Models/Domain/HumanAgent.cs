using System;
using System.Collections.Generic;
using backend.Models.Enums;

namespace backend.Models.Domain;

public class HumanAgent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OwnerUserId { get; set; }
    public Guid? ApplicationUserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public HumanAgentStatus Status { get; set; } = HumanAgentStatus.Offline;
    public bool IsActive { get; set; } = true;
    public int MaxConcurrentCalls { get; set; } = 1;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public User OwnerUser { get; set; } = null!;
    public User? ApplicationUser { get; set; }
    public ICollection<HumanAgentAccessKey> AccessKeys { get; set; } = new List<HumanAgentAccessKey>();
    public ICollection<HumanAgentSession> Sessions { get; set; } = new List<HumanAgentSession>();
    public ICollection<CallParticipant> CallParticipants { get; set; } = new List<CallParticipant>();
    public ICollection<CallTransfer> CallTransfers { get; set; } = new List<CallTransfer>();
    public ICollection<CallHandoff> CallHandoffs { get; set; } = new List<CallHandoff>();
}