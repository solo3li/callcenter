using System;
using backend.Models.Enums;

namespace backend.Models.Domain;

public class CallConfiguration
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? PersonaId { get; set; }
    public Guid? WorkflowId { get; set; }
    public bool IsActive { get; set; } = true;
    public string? ConfigJson { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
    public Persona? Persona { get; set; }
    public Workflow? Workflow { get; set; }
    public ICollection<CallConfigurationAction> CallConfigurationActions { get; set; } = new List<CallConfigurationAction>();
    public ICollection<CallSession> CallSessions { get; set; } = new List<CallSession>();
}