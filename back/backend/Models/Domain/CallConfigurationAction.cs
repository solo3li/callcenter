using System;

namespace backend.Models.Domain;

public class CallConfigurationAction
{
    public Guid CallConfigurationId { get; set; }
    public Guid ActionDefinitionId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public CallConfiguration CallConfiguration { get; set; } = null!;
    public ActionDefinition ActionDefinition { get; set; } = null!;
}