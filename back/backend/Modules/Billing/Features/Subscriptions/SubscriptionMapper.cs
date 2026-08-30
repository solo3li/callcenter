using backend.Dtos;
using backend.Modules.Billing.Models;

namespace backend.Modules.Billing.Features.Subscriptions;

public static class SubscriptionMapper
{
    public static SubscriptionDto Map(Subscription s) => new(
        s.Id, s.UserId, s.PlanId, s.Status.ToString(),
        s.StartsAt, s.EndsAt, s.TrialEndsAt,
        s.CreatedAt, s.UpdatedAt);
}
