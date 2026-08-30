using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediatR;
using backend.Data;
using backend.Dtos;

namespace backend.Modules.CallOperations.Features.HumanAgents.UpdateHumanAgent;

public record UpdateHumanAgentCommand(Guid Id, Guid OwnerUserId, UpdateHumanAgentRequest Request) : IRequest<HumanAgentListItem?>;

public class UpdateHumanAgentCommandHandler : IRequestHandler<UpdateHumanAgentCommand, HumanAgentListItem?>
{
    private readonly AppDbContext _db;

    public UpdateHumanAgentCommandHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<HumanAgentListItem?> Handle(UpdateHumanAgentCommand request, CancellationToken cancellationToken)
    {
        var agent = await _db.HumanAgents
            .FirstOrDefaultAsync(a => a.Id == request.Id && a.OwnerUserId == request.OwnerUserId && a.IsActive, cancellationToken);

        if (agent is null)
            return null;

        agent.Name = request.Request.Name;
        agent.Email = request.Request.Email;
        agent.MaxConcurrentCalls = request.Request.MaxConcurrentCalls;
        agent.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);

        return new HumanAgentListItem(
            agent.Id,
            agent.Name,
            agent.Email,
            agent.Status,
            agent.IsActive,
            agent.MaxConcurrentCalls,
            agent.CreatedAt,
            agent.UpdatedAt
        );
    }
}
