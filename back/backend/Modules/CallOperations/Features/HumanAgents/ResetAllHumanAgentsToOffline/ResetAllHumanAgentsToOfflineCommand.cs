using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediatR;
using backend.Data;
using backend.Models.Enums;

namespace backend.Modules.CallOperations.Features.HumanAgents.ResetAllHumanAgentsToOffline;

public record ResetAllHumanAgentsToOfflineCommand() : IRequest;

public class ResetAllHumanAgentsToOfflineCommandHandler : IRequestHandler<ResetAllHumanAgentsToOfflineCommand>
{
    private readonly AppDbContext _db;

    public ResetAllHumanAgentsToOfflineCommandHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task Handle(ResetAllHumanAgentsToOfflineCommand request, CancellationToken cancellationToken)
    {
        var agents = await _db.HumanAgents
            .Where(a => a.Status != HumanAgentStatus.Offline)
            .ToListAsync(cancellationToken);

        foreach (var agent in agents)
        {
            agent.Status = HumanAgentStatus.Offline;
            agent.UpdatedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(cancellationToken);
    }
}
