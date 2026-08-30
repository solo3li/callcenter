using backend.Modules.Identity.Dtos;
using backend.Modules.Billing.Dtos;
using backend.Modules.CallOperations.Dtos;
using backend.Modules.Configuration.Dtos;
using backend.Modules.Analytics.Dtos;
using backend.Modules.Billing.Models;

namespace backend.Modules.Billing.Features.Subscriptions;

public static class SubscriptionMapper
{
    public static SubscriptionDto Map(Subscription s) => new(
        s.Id, s.UserId, s.PlanId, s.Status.ToString(),
        s.StartsAt, s.EndsAt, s.TrialEndsAt,
        s.CreatedAt, s.UpdatedAt);
}
