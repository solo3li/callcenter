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
    public class PlanService
    {
        private readonly AppDbContext _db;

        public PlanService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<List<PlanDto>> ListAsync()
        {
            var plans = await _db.Plans.OrderBy(p => p.Name).ToListAsync();
            return plans.Select(Map).ToList();
        }

        public async Task<List<PlanDto>> ListActiveAsync()
        {
            var plans = await _db.Plans.Where(p => p.IsActive).OrderBy(p => p.Name).ToListAsync();
            return plans.Select(Map).ToList();
        }

        public async Task<PlanDto?> GetByIdAsync(Guid id)
        {
            var plan = await _db.Plans.FindAsync(id);
            return plan == null ? null : Map(plan);
        }

        public async Task<PlanDto> CreateAsync(CreatePlanRequest request)
        {
            if (!Enum.TryParse<PlanTier>(request.Tier, true, out var tier))
                throw new ArgumentException($"Invalid PlanTier: {request.Tier}");

            var plan = new Plan
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                Tier = tier,
                IsPlatformPlan = request.IsPlatformPlan,
                IsActive = true,
                EntitlementsJson = request.EntitlementsJson,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _db.Plans.Add(plan);
            await _db.SaveChangesAsync();
            return Map(plan);
        }

        public async Task<PlanDto?> UpdateAsync(Guid id, UpdatePlanRequest request)
        {
            var plan = await _db.Plans.FindAsync(id);
            if (plan == null) return null;

            if (request.Name != null) plan.Name = request.Name;
            if (request.Description != null) plan.Description = request.Description;
            if (request.Tier != null && Enum.TryParse<PlanTier>(request.Tier, true, out var tier)) plan.Tier = tier;
            if (request.IsActive.HasValue) plan.IsActive = request.IsActive.Value;
            if (request.EntitlementsJson != null) plan.EntitlementsJson = request.EntitlementsJson;
            plan.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return Map(plan);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var plan = await _db.Plans.FindAsync(id);
            if (plan == null) return false;
            plan.IsActive = false;
            plan.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<List<PartnerPlanDto>> ListPartnerPlansAsync(Guid partnerId)
        {
            var plans = await _db.PartnerPlans
                .Where(p => p.PartnerId == partnerId)
                .OrderBy(p => p.Name)
                .ToListAsync();
            return plans.Select(MapPartnerPlan).ToList();
        }

        public async Task<PartnerPlanDto?> GetPartnerPlanByIdAsync(Guid id)
        {
            var plan = await _db.PartnerPlans.FindAsync(id);
            return plan == null ? null : MapPartnerPlan(plan);
        }

        public async Task<PartnerPlanDto> CreatePartnerPlanAsync(Guid partnerId, CreatePartnerPlanRequest request)
        {
            var plan = new PartnerPlan
            {
                Id = Guid.NewGuid(),
                PartnerId = partnerId,
                Name = request.Name,
                Description = request.Description,
                IsActive = true,
                EntitlementsJson = request.EntitlementsJson,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _db.PartnerPlans.Add(plan);
            await _db.SaveChangesAsync();
            return MapPartnerPlan(plan);
        }

        public async Task<bool> DeletePartnerPlanAsync(Guid id)
        {
            var plan = await _db.PartnerPlans.FindAsync(id);
            if (plan == null) return false;
            plan.IsActive = false;
            plan.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return true;
        }

        private static PlanDto Map(Plan p) => new(
            p.Id, p.Name, p.Description, p.Tier.ToString(),
            p.IsPlatformPlan, p.IsActive, p.EntitlementsJson,
            p.CreatedAt, p.UpdatedAt);

        private static PartnerPlanDto MapPartnerPlan(PartnerPlan p) => new(
            p.Id, p.PartnerId, p.Name, p.Description,
            p.IsActive, p.EntitlementsJson, p.CreatedAt, p.UpdatedAt);
    }
}