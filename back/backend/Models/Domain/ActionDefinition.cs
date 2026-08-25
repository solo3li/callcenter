using System;
using System.Collections.Generic;
using backend.Models.Enums;

namespace backend.Models.Domain;

public class ActionDefinition
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ActionType ActionType { get; set; } = ActionType.System;
    public bool IsSystem { get; set; } = false;
    public string? InputSchemaJson { get; set; }
    public string? OutputSchemaJson { get; set; }
    public string? ConfigurationJson { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<PersonaAction> PersonaActions { get; set; } = new List<PersonaAction>();
    public ICollection<CallConfigurationAction> CallConfigurationActions { get; set; } = new List<CallConfigurationAction>();
    public ICollection<ActionExecution> ActionExecutions { get; set; } = new List<ActionExecution>();
}