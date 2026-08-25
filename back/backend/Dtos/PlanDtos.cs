using System;

namespace backend.Dtos
{
    public record PlanDto(
        Guid Id,
        string Name,
        string? Description,
        string Tier,
        bool IsPlatformPlan,
        bool IsActive,
        string? EntitlementsJson,
        DateTime CreatedAt,
        DateTime UpdatedAt
    );

    public record CreatePlanRequest(
        string Name,
        string? Description,
        string Tier,
        bool IsPlatformPlan,
        string? EntitlementsJson
    );

    public record UpdatePlanRequest(
        string? Name,
        string? Description,
        string? Tier,
        bool? IsActive,
        string? EntitlementsJson
    );

    public record PartnerPlanDto(
        Guid Id,
        Guid PartnerId,
        string Name,
        string? Description,
        bool IsActive,
        string? EntitlementsJson,
        DateTime CreatedAt,
        DateTime UpdatedAt
    );

    public record CreatePartnerPlanRequest(
        string Name,
        string? Description,
        string? EntitlementsJson
    );
}