using backend.Models.Enums;

namespace backend.Dtos;

public sealed record ActionDefinitionDto(
    Guid Id,
    string Name,
    string DisplayName,
    string? Description,
    ActionType ActionType,
    bool IsSystem,
    string? InputSchemaJson,
    string? OutputSchemaJson,
    string? ConfigurationJson,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public sealed record CreateActionRequest(
    string Name,
    string DisplayName,
    string? Description,
    ActionType ActionType,
    string? InputSchemaJson,
    string? OutputSchemaJson,
    string? ConfigurationJson
);

public sealed record UpdateActionRequest(
    string Name,
    string DisplayName,
    string? Description,
    string? InputSchemaJson,
    string? OutputSchemaJson,
    string? ConfigurationJson,
    bool IsActive
);