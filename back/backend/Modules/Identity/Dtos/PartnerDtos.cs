using System;

namespace backend.Modules.Identity.Dtos
{
    public record PartnerDto(
        Guid Id,
        Guid UserId,
        string OrganizationName,
        string? ContactEmail,
        string? PhoneNumber,
        string? Website,
        string? Description,
        bool IsActive,
        string? MetadataJson,
        DateTime CreatedAt,
        DateTime UpdatedAt
    );

    public record UpdatePartnerRequest(
        string? OrganizationName,
        string? ContactEmail,
        string? PhoneNumber,
        string? Website,
        string? Description,
        string? MetadataJson
    );

    public record PartnerRelationshipDto(
        Guid Id,
        Guid PartnerId,
        Guid CustomerUserId,
        string Status,
        string? MetadataJson,
        DateTime CreatedAt,
        DateTime UpdatedAt
    );

    public record CreateRelationshipRequest(
        Guid CustomerUserId,
        string? MetadataJson
    );

    public record ProvisionRequest(
        string ExternalCustomerId,
        string? Email,
        string? DisplayName,
        Guid? PartnerPlanId
    );

    public record ProvisionResponse(
        Guid PlatformUserId,
        Guid RelationshipId,
        Guid? LicenseId,
        string ApiKey
    );
}