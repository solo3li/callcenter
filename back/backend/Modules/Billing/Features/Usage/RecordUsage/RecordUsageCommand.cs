using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediatR;
using backend.Data;
using backend.Dtos;
using backend.Models.Enums;
using backend.Modules.Billing.Models;

namespace backend.Modules.Billing.Features.Usage.RecordUsage;

public record RecordUsageCommand(
    Guid UserId, 
    Guid? PartnerId, 
    Guid? LicenseId, 
    Guid? CallSessionId,
    string MetricType, 
    decimal Quantity, 
    string Unit) : IRequest<UsageRecordDto>;

public class RecordUsageCommandHandler : IRequestHandler<RecordUsageCommand, UsageRecordDto>
{
    private readonly AppDbContext _db;

    public RecordUsageCommandHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<UsageRecordDto> Handle(RecordUsageCommand command, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<MetricType>(command.MetricType, true, out var mt))
            throw new ArgumentException($"Invalid MetricType: {command.MetricType}");

        var idempotencyKey = $"{command.UserId}:{command.CallSessionId}:{command.MetricType}:{DateTime.UtcNow:yyyyMMddHHmm}";

        var existing = await _db.UsageRecords.FirstOrDefaultAsync(u => u.IdempotencyKey == idempotencyKey, cancellationToken);
        if (existing != null)
        {
            return UsageMapper.Map(existing);
        }

        var record = new UsageRecord
        {
            Id = Guid.NewGuid(),
            UserId = command.UserId,
            PartnerId = command.PartnerId,
            LicenseId = command.LicenseId,
            CallSessionId = command.CallSessionId,
            IdempotencyKey = idempotencyKey,
            MetricType = mt,
            Quantity = command.Quantity,
            Unit = command.Unit,
            OccurredAt = DateTime.UtcNow
        };

        _db.UsageRecords.Add(record);
        await _db.SaveChangesAsync(cancellationToken);

        return UsageMapper.Map(record);
    }
}
