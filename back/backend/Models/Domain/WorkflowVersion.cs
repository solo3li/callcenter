using System;

namespace backend.Models.Domain;

public class WorkflowVersion
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid WorkflowId { get; set; }
    public int VersionNumber { get; set; } = 1;
    public string DefinitionJson { get; set; } = "{}";
    public bool IsPublished { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Workflow Workflow { get; set; } = null!;
}