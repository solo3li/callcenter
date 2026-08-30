using backend.Modules.Identity.Dtos;
using backend.Modules.Billing.Dtos;
using backend.Modules.CallOperations.Dtos;
using backend.Modules.Configuration.Dtos;
using backend.Modules.Analytics.Dtos;
using backend.Modules.Billing.Models;

namespace backend.Modules.Billing.Features.Usage;

public static class UsageMapper
{
    public static UsageRecordDto Map(UsageRecord r) => new(
        r.Id, r.UserId, r.PartnerId, r.LicenseId, r.CallSessionId,
        r.IdempotencyKey, r.MetricType.ToString(), r.Quantity, r.Unit,
        r.OccurredAt, r.MetadataJson);
}
