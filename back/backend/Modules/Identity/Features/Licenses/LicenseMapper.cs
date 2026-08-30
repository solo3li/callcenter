using backend.Dtos;
using backend.Modules.Identity.Models;

namespace backend.Modules.Identity.Features.Licenses;

public static class LicenseMapper
{
    public static LicenseDto Map(License l) => new(
        l.Id, l.UserId, l.PartnerId, l.PartnerPlanId, l.Status.ToString(),
        l.StartsAt, l.EndsAt, l.LimitsJson, l.MetadataJson,
        l.CreatedAt, l.UpdatedAt);
}
