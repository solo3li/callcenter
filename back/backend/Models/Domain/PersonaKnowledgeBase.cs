using System;
using backend.Modules.Configuration.Models;

namespace backend.Models.Domain;

public class PersonaKnowledgeBase
{
    public Guid PersonaId { get; set; }
    public Guid KnowledgeBaseId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Persona Persona { get; set; } = null!;
    public KnowledgeBase KnowledgeBase { get; set; } = null!;
}
