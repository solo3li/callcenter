using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediatR;
using backend.Data;
using backend.Modules.Identity.Dtos;
using backend.Modules.Billing.Dtos;
using backend.Modules.CallOperations.Dtos;
using backend.Modules.Configuration.Dtos;
using backend.Modules.Analytics.Dtos;
using backend.Models.Enums;

namespace backend.Modules.CallOperations.Features.HumanAgents.UpdateHumanAgentStatus;

public record UpdateHumanAgentStatusCommand(Guid Id, Guid OwnerUserId, HumanAgentStatus Status) : IRequest<HumanAgentListItem?>;

public class UpdateHumanAgentStatusCommandHandler : IRequestHandler<UpdateHumanAgentStatusCommand, HumanAgentListItem?>
{
    private readonly AppDbContext _db;

    public UpdateHumanAgentStatusCommandHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<HumanAgentListItem?> Handle(UpdateHumanAgentStatusCommand request, CancellationToken cancellationToken)
    {
        var agent = await _db.HumanAgents
            .FirstOrDefaultAsync(a => a.Id == request.Id && a.OwnerUserId == request.OwnerUserId && a.IsActive, cancellationToken);

        if (agent is null)
            return null;

        agent.Status = request.Status;
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
