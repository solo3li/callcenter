using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using backend.Data;
using Microsoft.EntityFrameworkCore;
using System;

namespace backend.Hubs
{
    public class CallHub : Hub
    {
        private readonly AppDbContext _db;

        public CallHub(AppDbContext db)
        {
            _db = db;
        }

        public async Task RegisterAgent(string username)
        {
            var agent = await _db.Agents.FirstOrDefaultAsync(a => a.Username == username);
            if (agent != null)
            {
                agent.IsOnline = true;
                agent.Status = "Available";
                Context.Items["Username"] = username;
                await _db.SaveChangesAsync();
            }
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var username = Context.Items["Username"] as string;
            if (!string.IsNullOrEmpty(username))
            {
                var agent = await _db.Agents.FirstOrDefaultAsync(a => a.Username == username);
                if (agent != null)
                {
                    agent.IsOnline = false;
                    agent.Status = "Offline";
                    await _db.SaveChangesAsync();
                }
            }
            await base.OnDisconnectedAsync(exception);
        }
    }
}
