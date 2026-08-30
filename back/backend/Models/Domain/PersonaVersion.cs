using System;
using backend.Modules.Configuration.Models;

namespace backend.Models.Domain;

public class PersonaVersion
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PersonaId { get; set; }
    public int VersionNumber { get; set; } = 1;
    public string SystemPrompt { get; set; } = string.Empty;
    public string ConfigurationJson { get; set; } = "{}";
    public bool IsPublished { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Persona Persona { get; set; } = null!;
}
