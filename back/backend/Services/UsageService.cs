using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Dtos;
using backend.Models.Domain;
using backend.Models.Enums;

namespace backend.Services
{
    public class UsageService
    {
        private readonly AppDbContext _db;

        public UsageService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<List<UsageRecordDto>> ListAsync(Guid userId, UsageFilterRequest? filter = null)
        {
            var query = _db.UsageRecords.Where(u => u.UserId == userId);

            if (filter?.From.HasValue == true)
                query = query.Where(u => u.OccurredAt >= filter.From.Value);
            if (filter?.To.HasValue == true)
                query = query.Where(u => u.OccurredAt <= filter.To.Value);
            if (!string.IsNullOrEmpty(filter?.MetricType) && Enum.TryParse<MetricType>(filter.MetricType, true, out var mt))
                query = query.Where(u => u.MetricType == mt);
            if (filter?.CallSessionId.HasValue == true)
                query = query.Where(u => u.CallSessionId == filter.CallSessionId.Value);
            if (filter?.LicenseId.HasValue == true)
                query = query.Where(u => u.LicenseId == filter.LicenseId.Value);
            if (filter?.PartnerId.HasValue == true)
                query = query.Where(u => u.PartnerId == filter.PartnerId.Value);

            var records = await query.OrderByDescending(u => u.OccurredAt).Take(500).ToListAsync();

            return records.Select(r => new UsageRecordDto(
                r.Id, r.UserId, r.PartnerId, r.LicenseId, r.CallSessionId,
                r.IdempotencyKey, r.MetricType.ToString(), r.Quantity, r.Unit,
                r.OccurredAt, r.MetadataJson)).ToList();
        }

        public async Task<List<UsageSummaryDto>> GetSummaryAsync(Guid userId)
        {
            var now = DateTime.UtcNow;
            var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

            var summaries = await _db.UsageRecords
                .Where(u => u.UserId == userId && u.OccurredAt >= monthStart)
                .GroupBy(u => new { u.MetricType, u.Unit })
                .Select(g => new UsageSummaryDto(
                    g.Key.MetricType.ToString(),
                    g.Sum(u => u.Quantity),
                    g.Key.Unit,
                    g.Count()
                ))
                .ToListAsync();

            return summaries;
        }

        public async Task<List<UsageRecordDto>> GetByMetricTypeAsync(Guid userId, string metricType)
        {
            if (!Enum.TryParse<MetricType>(metricType, true, out var mt))
                return new List<UsageRecordDto>();

            var records = await _db.UsageRecords
                .Where(u => u.UserId == userId && u.MetricType == mt)
                .OrderByDescending(u => u.OccurredAt)
                .Take(500)
                .ToListAsync();

            return records.Select(r => new UsageRecordDto(
                r.Id, r.UserId, r.PartnerId, r.LicenseId, r.CallSessionId,
                r.IdempotencyKey, r.MetricType.ToString(), r.Quantity, r.Unit,
                r.OccurredAt, r.MetadataJson)).ToList();
        }

        public async Task<List<UsageRecordDto>> GetByCallAsync(Guid userId, Guid callSessionId)
        {
            var records = await _db.UsageRecords
                .Where(u => u.UserId == userId && u.CallSessionId == callSessionId)
                .OrderByDescending(u => u.OccurredAt)
                .ToListAsync();

            return records.Select(r => new UsageRecordDto(
                r.Id, r.UserId, r.PartnerId, r.LicenseId, r.CallSessionId,
                r.IdempotencyKey, r.MetricType.ToString(), r.Quantity, r.Unit,
                r.OccurredAt, r.MetadataJson)).ToList();
        }

        public async Task<UsageRecordDto?> RecordUsageAsync(
            Guid userId, Guid? partnerId, Guid? licenseId, Guid? callSessionId,
            string metricType, decimal quantity, string unit)
        {
            if (!Enum.TryParse<MetricType>(metricType, true, out var mt))
                throw new ArgumentException($"Invalid MetricType: {metricType}");

            var idempotencyKey = $"{userId}:{callSessionId}:{metricType}:{DateTime.UtcNow:yyyyMMddHHmm}";

            var existing = await _db.UsageRecords.FirstOrDefaultAsync(u => u.IdempotencyKey == idempotencyKey);
            if (existing != null)
            {
                return new UsageRecordDto(
                    existing.Id, existing.UserId, existing.PartnerId, existing.LicenseId,
                    existing.CallSessionId, existing.IdempotencyKey,
                    existing.MetricType.ToString(), existing.Quantity, existing.Unit,
                    existing.OccurredAt, existing.MetadataJson);
            }

            var record = new UsageRecord
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                PartnerId = partnerId,
                LicenseId = licenseId,
                CallSessionId = callSessionId,
                IdempotencyKey = idempotencyKey,
                MetricType = mt,
                Quantity = quantity,
                Unit = unit,
                OccurredAt = DateTime.UtcNow
            };

            _db.UsageRecords.Add(record);
            await _db.SaveChangesAsync();

            return new UsageRecordDto(
                record.Id, record.UserId, record.PartnerId, record.LicenseId,
                record.CallSessionId, record.IdempotencyKey,
                record.MetricType.ToString(), record.Quantity, record.Unit,
                record.OccurredAt, record.MetadataJson);
        }
    }
}