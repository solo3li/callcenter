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
    public class SubscriptionService
    {
        private readonly AppDbContext _db;

        public SubscriptionService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<List<SubscriptionDto>> ListAsync(Guid userId)
        {
            var subs = await _db.Subscriptions
                .Where(s => s.UserId == userId)
                .Include(s => s.Plan)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
            return subs.Select(Map).ToList();
        }

        public async Task<SubscriptionDto?> GetByIdAsync(Guid id)
        {
            var sub = await _db.Subscriptions
                .Include(s => s.Plan)
                .FirstOrDefaultAsync(s => s.Id == id);
            return sub == null ? null : Map(sub);
        }

        public async Task<SubscriptionDto> CreateAsync(Guid userId, CreateSubscriptionRequest request)
        {
            var plan = await _db.Plans.FindAsync(request.PlanId);
            if (plan == null)
                throw new ArgumentException("Plan not found");

            var sub = new Subscription
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                PlanId = request.PlanId,
                Status = SubscriptionStatus.Trialing,
                StartsAt = request.StartsAt,
                EndsAt = request.EndsAt,
                TrialEndsAt = request.TrialEndsAt,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _db.Subscriptions.Add(sub);
            await _db.SaveChangesAsync();

            await _db.Entry(sub).Reference(s => s.Plan).LoadAsync();
            return Map(sub);
        }

        public async Task<SubscriptionDto?> UpdateAsync(Guid id, UpdateSubscriptionRequest request)
        {
            var sub = await _db.Subscriptions
                .Include(s => s.Plan)
                .FirstOrDefaultAsync(s => s.Id == id);
            if (sub == null) return null;

            if (request.Status != null && Enum.TryParse<SubscriptionStatus>(request.Status, true, out var status))
                sub.Status = status;
            if (request.EndsAt.HasValue)
                sub.EndsAt = request.EndsAt.Value;
            sub.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return Map(sub);
        }

        public async Task<bool> CancelAsync(Guid id)
        {
            var sub = await _db.Subscriptions.FindAsync(id);
            if (sub == null) return false;
            sub.Status = SubscriptionStatus.Cancelled;
            sub.EndsAt = DateTime.UtcNow;
            sub.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return true;
        }

        private static SubscriptionDto Map(Subscription s) => new(
            s.Id, s.UserId, s.PlanId, s.Status.ToString(),
            s.StartsAt, s.EndsAt, s.TrialEndsAt,
            s.CreatedAt, s.UpdatedAt);
    }
}