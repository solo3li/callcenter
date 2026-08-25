using System;
using backend.Models.Enums;

namespace backend.Models.Domain;

public class ActionExecution
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CallSessionId { get; set; }
    public Guid ActionDefinitionId { get; set; }
    public Guid? WorkflowExecutionId { get; set; }
    public ActionExecutionStatus Status { get; set; } = ActionExecutionStatus.Pending;
    public string? InputJson { get; set; }
    public string? OutputJson { get; set; }
    public string? Error { get; set; }
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }

    public CallSession CallSession { get; set; } = null!;
    public ActionDefinition ActionDefinition { get; set; } = null!;
    public WorkflowExecution? WorkflowExecution { get; set; }
}