using backend.Dtos;
using backend.Modules.Billing.Models;

namespace backend.Modules.Billing.Features.Plans;

public static class PlanMapper
{
    public static PlanDto Map(Plan p) => new(
        p.Id, p.Name, p.Description, p.Tier.ToString(),
        p.IsPlatformPlan, p.IsActive, p.EntitlementsJson,
        p.CreatedAt, p.UpdatedAt);
}
