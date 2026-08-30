using backend.Models.Enums;

namespace backend.Modules.Identity.Dtos;

public sealed record HumanAgentListItem(
    Guid Id,
    string Name,
    string? Email,
    HumanAgentStatus Status,
    bool IsActive,
    int MaxConcurrentCalls,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public sealed record CreateHumanAgentRequest(
    string Name,
    string? Email,
    int MaxConcurrentCalls = 1
);

public sealed record UpdateHumanAgentRequest(
    string Name,
    string? Email,
    int MaxConcurrentCalls
);

public sealed record UpdateAgentStatusRequest(
    HumanAgentStatus Status
);

public sealed record CreateAccessKeyRequest(
    string Name,
    DateTime? ExpiresAt = null
);

public sealed record CreateAccessKeyResponse(
    Guid Id,
    string Name,
    string RawKey,
    string KeyPrefix,
    DateTime? ExpiresAt,
    DateTime CreatedAt
);

public sealed record AccessKeyListItem(
    Guid Id,
    string Name,
    string KeyPrefix,
    AccessKeyStatus Status,
    DateTime? ExpiresAt,
    DateTime? LastUsedAt,
    DateTime? RevokedAt,
    DateTime CreatedAt
);

public sealed record AgentSessionDto(
    Guid Id,
    Guid HumanAgentId,
    string LivekitIdentity,
    string Status,
    DateTime ConnectedAt,
    DateTime? DisconnectedAt,
    DateTime? LastHeartbeatAt,
    string? MetadataJson
);