using System;

namespace backend.Dtos
{
    public record WorkflowListItem(
        Guid Id,
        string Name,
        string? Description,
        bool IsActive,
        int VersionCount,
        DateTime CreatedAt,
        DateTime UpdatedAt
    );

    public record CreateWorkflowRequest(
        string Name,
        string? Description
    );

    public record UpdateWorkflowRequest(
        string? Name,
        string? Description,
        bool? IsActive
    );

    public record WorkflowVersionDto(
        Guid Id,
        Guid WorkflowId,
        int VersionNumber,
        string DefinitionJson,
        bool IsPublished,
        DateTime CreatedAt
    );

    public record CreateWorkflowVersionRequest(
        string DefinitionJson
    );
}