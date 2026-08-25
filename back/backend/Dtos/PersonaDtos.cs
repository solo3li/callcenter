namespace backend.Dtos;

public sealed record PersonaListItem(
    Guid Id,
    string Name,
    string? Description,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public sealed record CreatePersonaRequest(
    string Name,
    string? Description
);

public sealed record UpdatePersonaRequest(
    string Name,
    string? Description,
    bool IsActive
);

public sealed record PersonaVersionDto(
    Guid Id,
    Guid PersonaId,
    int VersionNumber,
    string SystemPrompt,
    string ConfigurationJson,
    bool IsPublished,
    DateTime CreatedAt
);

public sealed record CreatePersonaVersionRequest(
    string? SystemPrompt,
    string? ConfigurationJson
);

public sealed record PublishVersionRequest(
    Guid PersonaId,
    Guid VersionId
);