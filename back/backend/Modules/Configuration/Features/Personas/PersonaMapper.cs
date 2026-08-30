using backend.Dtos;
using backend.Modules.Configuration.Models;

namespace backend.Modules.Configuration.Features.Personas;

public static class PersonaMapper
{
    public static PersonaListItem Map(Persona p) => new(
        p.Id, p.Name, p.Description, p.IsActive, p.CreatedAt, p.UpdatedAt);

    public static PersonaVersionDto MapVersion(PersonaVersion v) => new(
        v.Id, v.PersonaId, v.VersionNumber, v.SystemPrompt, v.ConfigurationJson,
        v.IsPublished, v.CreatedAt);
}
