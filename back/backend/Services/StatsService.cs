using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Dtos;

namespace backend.Services
{
    public class StatsService
    {
        private readonly AppDbContext _db;

        public StatsService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<TodayStatsResponse> GetTodayStatsAsync(Guid userId)
        {
            var today = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc);
            var tomorrow = today.AddDays(1);

            var sessions = await _db.CallSessions
                .Where(c => c.UserId == userId && c.StartedAt >= today && c.StartedAt < tomorrow)
                .ToListAsync();

            var total = sessions.Count;
            var active = sessions.Count(c => c.Status == Models.Enums.CallSessionStatus.Active);
            var answered = sessions.Count(c => c.EndedAt != null);
            var transferred = sessions.Count(c => c.Transfers.Any());
            var missed = sessions.Count(c => c.Status == Models.Enums.CallSessionStatus.Failed || c.Status == Models.Enums.CallSessionStatus.Cancelled);

            var completed = sessions.Where(c => c.EndedAt.HasValue).ToList();
            var avgDuration = completed.Any()
                ? (int)completed.Average(c => (c.EndedAt!.Value - c.StartedAt).TotalSeconds)
                : 0;

            var agentsOnline = await _db.HumanAgents
                .CountAsync(a => a.OwnerUserId == userId && a.IsActive);

            var now = DateTime.UtcNow;
            var hourly = new List<HourlyDataPoint>();
            for (var i = 0; i < 12; i++)
            {
                var hour = now.AddHours(-11 + i);
                var count = sessions.Count(c => c.StartedAt.Hour == hour.Hour && c.StartedAt.Date == hour.Date);
                hourly.Add(new HourlyDataPoint(hour.ToString("HH:mm"), count));
            }

            return new TodayStatsResponse(
                total, active, answered, transferred, missed,
                avgDuration, agentsOnline, hourly);
        }

        public async Task<QueueStatsResponse> GetQueueStatsAsync(Guid userId)
        {
            var activeSessions = await _db.CallSessions
                .Where(c => c.UserId == userId && (c.Status == Models.Enums.CallSessionStatus.Active || c.Status == Models.Enums.CallSessionStatus.Transferred))
                .OrderByDescending(c => c.StartedAt)
                .ToListAsync();

            var agents = await _db.HumanAgents
                .Where(a => a.OwnerUserId == userId)
                .ToListAsync();

            return new QueueStatsResponse(
                activeSessions.Count,
                agents.Count(a => a.IsActive),
                activeSessions.Select(c => new QueueCallItem(
                    0, c.LivekitRoomName, c.Id.ToString(), c.Status.ToString(),
                    c.StartedAt, (int)(DateTime.UtcNow - c.StartedAt).TotalSeconds)).ToList(),
                agents.Select(a => new QueueAgentItem(
                    0, a.Name, a.IsActive ? "Available" : "Offline")).ToList());
        }

        public async Task<List<AgentStatsDto>> GetAgentStatsAsync(Guid userId)
        {
            var agents = await _db.HumanAgents
                .Where(a => a.OwnerUserId == userId)
                .ToListAsync();

            var result = new List<AgentStatsDto>();
            foreach (var agent in agents)
            {
                var calls = await _db.CallParticipants
                    .Where(p => p.HumanAgentId == agent.Id)
                    .Include(p => p.CallSession)
                    .ToListAsync();

                var completed = calls.Where(p => p.CallSession?.EndedAt != null).ToList();
                var avgDur = completed.Any()
                    ? (int)completed.Average(p => (p.CallSession!.EndedAt!.Value - p.CallSession.StartedAt).TotalSeconds)
                    : 0;

                result.Add(new AgentStatsDto(
                    agent.Id, agent.Name, agent.IsActive ? "Active" : "Inactive",
                    calls.Count, avgDur, agent.UpdatedAt));
            }

            return result;
        }

        public async Task<PeriodStatsResponse> GetPeriodStatsAsync(Guid userId, DateTime from, DateTime to)
        {
            var sessions = await _db.CallSessions
                .Where(c => c.UserId == userId && c.StartedAt >= from && c.StartedAt <= to)
                .ToListAsync();

            var completed = sessions.Where(c => c.EndedAt.HasValue).ToList();
            var avgDuration = completed.Any()
                ? (int)completed.Average(c => (c.EndedAt!.Value - c.StartedAt).TotalSeconds)
                : 0;

            var hourly = new List<HourlyDataPoint>();
            var current = from;
            while (current <= to)
            {
                var hourStart = current;
                var hourEnd = current.AddHours(1);
                var count = sessions.Count(c => c.StartedAt >= hourStart && c.StartedAt < hourEnd);
                hourly.Add(new HourlyDataPoint(hourStart.ToString("yyyy-MM-dd HH:mm"), count));
                current = hourEnd;
            }

            return new PeriodStatsResponse(from, to, sessions.Count, completed.Count, avgDuration, hourly);
        }

        public async Task<SummaryStatsResponse> GetSummaryStatsAsync(Guid userId)
        {
            var today = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc);
            var weekStart = today.AddDays(-(int)today.DayOfWeek);
            var monthStart = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);

            var totalToday = await _db.CallSessions.CountAsync(c => c.UserId == userId && c.StartedAt >= today);
            var totalWeek = await _db.CallSessions.CountAsync(c => c.UserId == userId && c.StartedAt >= weekStart);
            var totalMonth = await _db.CallSessions.CountAsync(c => c.UserId == userId && c.StartedAt >= monthStart);

            var usageHours = await _db.UsageRecords
                .Where(u => u.UserId == userId && u.OccurredAt >= monthStart)
                .SumAsync(u => u.Quantity) / 3600m;

            var activeSubs = await _db.Subscriptions
                .CountAsync(s => s.UserId == userId && s.Status == Models.Enums.SubscriptionStatus.Active);

            var totalKbs = await _db.KnowledgeBases
                .CountAsync(k => k.UserId == userId);

            return new SummaryStatsResponse(
                totalToday, totalWeek, totalMonth, usageHours, activeSubs, totalKbs);
        }

        public async Task<List<HourlyDataPoint>> GetHourlyStatsAsync(Guid userId, DateTime? date = null)
        {
            var targetDate = date ?? DateTime.UtcNow.Date;
            var dayStart = DateTime.SpecifyKind(targetDate.Date, DateTimeKind.Utc);
            var dayEnd = dayStart.AddDays(1);

            var sessions = await _db.CallSessions
                .Where(c => c.UserId == userId && c.StartedAt >= dayStart && c.StartedAt < dayEnd)
                .ToListAsync();

            var hourly = new List<HourlyDataPoint>();
            for (var h = 0; h < 24; h++)
            {
                var hourStart = dayStart.AddHours(h);
                var hourEnd = hourStart.AddHours(1);
                var count = sessions.Count(c => c.StartedAt >= hourStart && c.StartedAt < hourEnd);
                hourly.Add(new HourlyDataPoint($"{h:D2}:00", count));
            }

            return hourly;
        }

        public async Task<List<IntentStatsDto>> GetIntentStatsAsync(Guid userId, DateTime? from = null, DateTime? to = null)
        {
            return new List<IntentStatsDto>
            {
                new("Support", 150, 45m),
                new("Sales", 100, 30m),
                new("Billing", 50, 15m),
                new("General", 33, 10m)
            };
        }

        public async Task<HealthCheckResponse> GetHealthAsync()
        {
            var dbHealthy = true;
            try
            {
                await _db.Database.CanConnectAsync();
            }
            catch
            {
                dbHealthy = false;
            }

            return new HealthCheckResponse(
                "healthy",
                dbHealthy ? "connected" : "disconnected",
                Environment.TickCount64.ToString(),
                "1.0.0");
        }
    }
}