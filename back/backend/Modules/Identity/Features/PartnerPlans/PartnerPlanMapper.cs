using backend.Modules.Identity.Dtos;
using backend.Modules.Billing.Dtos;
using backend.Modules.CallOperations.Dtos;
using backend.Modules.Configuration.Dtos;
using backend.Modules.Analytics.Dtos;
using backend.Modules.Identity.Models;

namespace backend.Modules.Identity.Features.PartnerPlans;

public static class PartnerPlanMapper
{
    public static PartnerPlanDto Map(PartnerPlan p) => new(
        p.Id, p.PartnerId, p.Name, p.Description,
        p.IsActive, p.EntitlementsJson, p.CreatedAt, p.UpdatedAt);
}
