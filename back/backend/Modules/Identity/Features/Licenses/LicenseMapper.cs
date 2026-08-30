using backend.Modules.Identity.Dtos;
using backend.Modules.Billing.Dtos;
using backend.Modules.CallOperations.Dtos;
using backend.Modules.Configuration.Dtos;
using backend.Modules.Analytics.Dtos;
using backend.Modules.Identity.Models;

namespace backend.Modules.Identity.Features.Licenses;

public static class LicenseMapper
{
    public static LicenseDto Map(License l) => new(
        l.Id, l.UserId, l.PartnerId, l.PartnerPlanId, l.Status.ToString(),
        l.StartsAt, l.EndsAt, l.LimitsJson, l.MetadataJson,
        l.CreatedAt, l.UpdatedAt);
}
