using backend.Models.Domain;

namespace backend.Modules.Configuration.Dtos
{
    public sealed record CallConfigListItem(
        Guid Id,
        string Name,
        string? Description,
        Guid? PersonaId,
        string? PersonaName,
        Guid? WorkflowId,
        string? WorkflowName,
        bool IsActive,
        string? ConfigJson,
        int ActionCount,
        DateTime CreatedAt,
        DateTime UpdatedAt
    );

    public sealed record CreateCallConfigRequest(
        string Name,
        string? Description = null,
        Guid? PersonaId = null,
        Guid? WorkflowId = null,
        string? ConfigJson = null
    );

    public sealed record UpdateCallConfigRequest(
        string Name,
        string? Description = null,
        Guid? PersonaId = null,
        Guid? WorkflowId = null,
        string? ConfigJson = null,
        bool? IsActive = null
    );

    public sealed record SetConfigActionsRequest(
        List<Guid> ActionDefinitionIds
    );
}