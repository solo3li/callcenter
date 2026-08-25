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
    public class LicenseService
    {
        private readonly AppDbContext _db;

        public LicenseService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<List<LicenseDto>> ListAsync(Guid? userId = null, Guid? partnerId = null)
        {
            var query = _db.Licenses.AsQueryable();
            if (userId.HasValue)
                query = query.Where(l => l.UserId == userId.Value);
            if (partnerId.HasValue)
                query = query.Where(l => l.PartnerId == partnerId.Value);

            var licenses = await query.OrderByDescending(l => l.CreatedAt).ToListAsync();
            return licenses.Select(Map).ToList();
        }

        public async Task<LicenseDto?> GetByIdAsync(Guid id)
        {
            var license = await _db.Licenses.FindAsync(id);
            return license == null ? null : Map(license);
        }

        public async Task<LicenseDto> CreateAsync(CreateLicenseRequest request)
        {
            var license = new License
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                PartnerId = request.PartnerId,
                PartnerPlanId = request.PartnerPlanId,
                Status = LicenseStatus.Active,
                StartsAt = request.StartsAt,
                EndsAt = request.EndsAt,
                LimitsJson = request.LimitsJson,
                MetadataJson = request.MetadataJson,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _db.Licenses.Add(license);
            await _db.SaveChangesAsync();
            return Map(license);
        }

        public async Task<LicenseDto?> UpdateAsync(Guid id, UpdateLicenseRequest request)
        {
            var license = await _db.Licenses.FindAsync(id);
            if (license == null) return null;

            if (request.Status != null && Enum.TryParse<LicenseStatus>(request.Status, true, out var status))
                license.Status = status;
            if (request.EndsAt.HasValue)
                license.EndsAt = request.EndsAt.Value;
            if (request.LimitsJson != null)
                license.LimitsJson = request.LimitsJson;
            if (request.MetadataJson != null)
                license.MetadataJson = request.MetadataJson;
            license.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return Map(license);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var license = await _db.Licenses.FindAsync(id);
            if (license == null) return false;
            license.Status = LicenseStatus.Cancelled;
            license.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return true;
        }

        private static LicenseDto Map(License l) => new(
            l.Id, l.UserId, l.PartnerId, l.PartnerPlanId, l.Status.ToString(),
            l.StartsAt, l.EndsAt, l.LimitsJson, l.MetadataJson,
            l.CreatedAt, l.UpdatedAt);
    }
}