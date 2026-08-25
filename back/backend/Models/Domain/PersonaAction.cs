using System;
using System.Collections.Generic;

namespace backend.Models.Domain;

public class PersonaAction
{
    public Guid PersonaId { get; set; }
    public Guid ActionDefinitionId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Persona Persona { get; set; } = null!;
    public ActionDefinition ActionDefinition { get; set; } = null!;
}