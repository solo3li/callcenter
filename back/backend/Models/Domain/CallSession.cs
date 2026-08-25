using System;
using System.Collections.Generic;
using backend.Models.Enums;

namespace backend.Models.Domain;

public class CallSession
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public Guid? CallConfigurationId { get; set; }
    public Guid? PersonaVersionId { get; set; }
    public Guid? WorkflowVersionId { get; set; }
    public Guid? ApiKeyId { get; set; }
    public string LivekitRoomName { get; set; } = string.Empty;
    public string? LivekitRoomSid { get; set; }
    public CallSessionStatus Status { get; set; } = CallSessionStatus.Queued;
    public CallDirection Direction { get; set; } = CallDirection.Inbound;
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? AnsweredAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public int? DurationSeconds { get; set; }
    public string? MetadataJson { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
    public CallConfiguration? CallConfiguration { get; set; }
    public PersonaVersion? PersonaVersion { get; set; }
    public WorkflowVersion? WorkflowVersion { get; set; }
    public ApiKey? ApiKey { get; set; }

    public ICollection<CallParticipant> Participants { get; set; } = new List<CallParticipant>();
    public ICollection<CallTransfer> Transfers { get; set; } = new List<CallTransfer>();
    public ICollection<CallHandoff> Handoffs { get; set; } = new List<CallHandoff>();
    public ICollection<CallRecording> Recordings { get; set; } = new List<CallRecording>();
    public ICollection<ActionExecution> ActionExecutions { get; set; } = new List<ActionExecution>();
    public ICollection<WorkflowExecution> WorkflowExecutions { get; set; } = new List<WorkflowExecution>();
    public ICollection<UsageRecord> UsageRecords { get; set; } = new List<UsageRecord>();
}