using System;

namespace backend.Modules.Billing.Dtos
{
    public record UsageRecordDto(
        Guid Id,
        Guid UserId,
        Guid? PartnerId,
        Guid? LicenseId,
        Guid? CallSessionId,
        string IdempotencyKey,
        string MetricType,
        decimal Quantity,
        string Unit,
        DateTime OccurredAt,
        string? MetadataJson
    );

    public record UsageSummaryDto(
        string MetricType,
        decimal TotalQuantity,
        string Unit,
        int Count
    );

    public record UsageFilterRequest(
        DateTime? From,
        DateTime? To,
        string? MetricType,
        Guid? CallSessionId,
        Guid? LicenseId,
        Guid? PartnerId
    );
}