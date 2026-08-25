using System;
using System.Collections.Generic;

namespace backend.Models.Domain;

public class Persona
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
    public ICollection<PersonaVersion> Versions { get; set; } = new List<PersonaVersion>();
    public ICollection<PersonaAction> PersonaActions { get; set; } = new List<PersonaAction>();
    public ICollection<PersonaKnowledgeBase> PersonaKnowledgeBases { get; set; } = new List<PersonaKnowledgeBase>();
}