using System;

namespace backend.Models.Domain;

public class WorkflowExecution
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid WorkflowVersionId { get; set; }
    public Guid? CallSessionId { get; set; }
    public string Status { get; set; } = "pending";
    public string? InputJson { get; set; }
    public string? OutputJson { get; set; }
    public string? StateJson { get; set; }
    public string? Error { get; set; }
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }

    public WorkflowVersion WorkflowVersion { get; set; } = null!;
    public CallSession? CallSession { get; set; }
}