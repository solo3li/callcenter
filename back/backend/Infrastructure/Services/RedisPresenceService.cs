using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace backend.Services
{
    public class RedisPresenceService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IDatabase _db;

        public RedisPresenceService(IConnectionMultiplexer redis)
        {
            _redis = redis;
            _db = redis.GetDatabase();
        }

        public async Task SetAgentStatusAsync(Guid ownerUserId, Guid agentId, string status)
        {
            var key = $"agent:{ownerUserId}:{agentId}";
            await _db.HashSetAsync(key, new HashEntry[]
            {
                new("status", status),
                new("last_heartbeat", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString())
            });
            await _db.KeyExpireAsync(key, TimeSpan.FromMinutes(5));
        }

        public async Task<List<Guid>> GetAvailableAgentsAsync(Guid ownerUserId)
        {
            var server = _redis.GetServer(_redis.GetEndPoints()[0]);
            var pattern = $"agent:{ownerUserId}:*";
            var keys = server.Keys(pattern: pattern);
            var available = new List<Guid>();

            foreach (var key in keys)
            {
                var status = await _db.HashGetAsync(key, "status");
                if (status == "Available" || status == "Available")
                {
                    var parts = key.ToString().Split(':');
                    if (parts.Length == 3 && Guid.TryParse(parts[2], out var agentId))
                        available.Add(agentId);
                }
            }
            return available;
        }

        public async Task<bool> AcquireTransferLockAsync(Guid agentId)
        {
            return await _db.StringSetAsync($"transfer_lock:{agentId}", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), TimeSpan.FromSeconds(30), When.NotExists);
        }

        public async Task ReleaseTransferLockAsync(Guid agentId)
        {
            await _db.KeyDeleteAsync($"transfer_lock:{agentId}");
        }

        public async Task<bool> IsHealthyAsync()
        {
            try
            {
                await _db.PingAsync();
                return true;
            }
            catch { return false; }
        }
    }
}