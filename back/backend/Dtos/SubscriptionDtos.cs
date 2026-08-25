using System;

namespace backend.Dtos
{
    public record SubscriptionDto(
        Guid Id,
        Guid UserId,
        Guid PlanId,
        string Status,
        DateTime StartsAt,
        DateTime? EndsAt,
        DateTime? TrialEndsAt,
        DateTime CreatedAt,
        DateTime UpdatedAt
    );

    public record CreateSubscriptionRequest(
        Guid PlanId,
        DateTime StartsAt,
        DateTime? EndsAt,
        DateTime? TrialEndsAt
    );

    public record UpdateSubscriptionRequest(
        string? Status,
        DateTime? EndsAt
    );
}