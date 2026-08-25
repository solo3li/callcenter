using System;

namespace backend.Dtos
{
    public record LicenseDto(
        Guid Id,
        Guid UserId,
        Guid? PartnerId,
        Guid? PartnerPlanId,
        string Status,
        DateTime StartsAt,
        DateTime? EndsAt,
        string? LimitsJson,
        string? MetadataJson,
        DateTime CreatedAt,
        DateTime UpdatedAt
    );

    public record CreateLicenseRequest(
        Guid UserId,
        Guid? PartnerId,
        Guid? PartnerPlanId,
        DateTime StartsAt,
        DateTime? EndsAt,
        string? LimitsJson,
        string? MetadataJson
    );

    public record UpdateLicenseRequest(
        string? Status,
        DateTime? EndsAt,
        string? LimitsJson,
        string? MetadataJson
    );
}