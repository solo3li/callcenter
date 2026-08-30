using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediatR;
using backend.Data;

namespace backend.Modules.CallOperations.Features.HumanAgents.DeleteHumanAgent;

public record DeleteHumanAgentCommand(Guid Id, Guid OwnerUserId) : IRequest<bool>;

public class DeleteHumanAgentCommandHandler : IRequestHandler<DeleteHumanAgentCommand, bool>
{
    private readonly AppDbContext _db;

    public DeleteHumanAgentCommandHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<bool> Handle(DeleteHumanAgentCommand request, CancellationToken cancellationToken)
    {
        var agent = await _db.HumanAgents
            .FirstOrDefaultAsync(a => a.Id == request.Id && a.OwnerUserId == request.OwnerUserId && a.IsActive, cancellationToken);

        if (agent is null)
            return false;

        agent.IsActive = false;
        agent.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);

        return true;
    }
}
