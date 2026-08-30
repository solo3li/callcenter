using backend.Modules.Identity.Dtos;
using backend.Modules.Billing.Dtos;
using backend.Modules.CallOperations.Dtos;
using backend.Modules.Configuration.Dtos;
using backend.Modules.Analytics.Dtos;
using backend.Modules.Identity.Models;

namespace backend.Modules.Identity.Features.Partners;

public static class PartnerMapper
{
    public static PartnerDto Map(Partner p) => new(
        p.Id, p.UserId, p.OrganizationName, p.ContactEmail,
        p.PhoneNumber, p.Website, p.Description,
        p.IsActive, p.MetadataJson, p.CreatedAt, p.UpdatedAt);

    public static PartnerRelationshipDto MapRelation(PartnerRelationship r) => new(
        r.Id, r.PartnerId, r.CustomerUserId, r.Status.ToString(),
        r.MetadataJson, r.CreatedAt, r.UpdatedAt);
}
