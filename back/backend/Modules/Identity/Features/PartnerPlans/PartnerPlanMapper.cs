using backend.Dtos;
using backend.Modules.Identity.Models;

namespace backend.Modules.Identity.Features.PartnerPlans;

public static class PartnerPlanMapper
{
    public static PartnerPlanDto Map(PartnerPlan p) => new(
        p.Id, p.PartnerId, p.Name, p.Description,
        p.IsActive, p.EntitlementsJson, p.CreatedAt, p.UpdatedAt);
}
