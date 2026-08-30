using backend.Modules.Identity.Dtos;
using backend.Modules.Billing.Dtos;
using backend.Modules.CallOperations.Dtos;
using backend.Modules.Configuration.Dtos;
using backend.Modules.Analytics.Dtos;
using backend.Modules.Billing.Models;

namespace backend.Modules.Billing.Features.Plans;

public static class PlanMapper
{
    public static PlanDto Map(Plan p) => new(
        p.Id, p.Name, p.Description, p.Tier.ToString(),
        p.IsPlatformPlan, p.IsActive, p.EntitlementsJson,
        p.CreatedAt, p.UpdatedAt);
}
