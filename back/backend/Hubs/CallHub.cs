using Microsoft.AspNetCore.SignalR;
using backend.Data;
using backend.Models.Domain;
using backend.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace backend.Hubs
{
    public class CallHub : Hub
    {
        private readonly AppDbContext _db;

        public CallHub(AppDbContext db) { _db = db; }

        public async Task RegisterAgent(string agentId)
        {
            if (!Guid.TryParse(agentId, out var id)) return;
            var agent = await _db.HumanAgents.FirstOrDefaultAsync(a => a.Id == id);
            if (agent == null) return;

            agent.Status = HumanAgentStatus.Available;
            Context.Items["HumanAgentId"] = agent.Id;
            Context.Items["OwnerUserId"] = agent.OwnerUserId;

            await Groups.AddToGroupAsync(Context.ConnectionId, $"agent_{agent.Id}");

            var session = new HumanAgentSession
            {
                HumanAgentId = agent.Id,
                LivekitIdentity = $"agent_{agent.Id}",
                Status = "active",
                ConnectedAt = DateTime.UtcNow
            };
            _db.HumanAgentSessions.Add(session);
            await _db.SaveChangesAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var agentId = Context.Items["HumanAgentId"] as Guid?;
            if (agentId.HasValue)
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"agent_{agentId.Value}");

                var agent = await _db.HumanAgents.FirstOrDefaultAsync(a => a.Id == agentId.Value);
                if (agent != null)
                {
                    agent.Status = HumanAgentStatus.Offline;
                }
                var session = await _db.HumanAgentSessions
                    .Where(s => s.HumanAgentId == agentId.Value && s.DisconnectedAt == null)
                    .OrderByDescending(s => s.ConnectedAt)
                    .FirstOrDefaultAsync();
                if (session != null)
                {
                    session.DisconnectedAt = DateTime.UtcNow;
                    session.Status = "disconnected";
                }
                await _db.SaveChangesAsync();
            }
            await base.OnDisconnectedAsync(exception);
        }
    }
}