using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediatR;
using backend.Data;
using backend.Dtos;
using backend.Models.Domain;
using backend.Models.Enums;

namespace backend.Modules.CallOperations.Features.HumanAgents.CreateHumanAgent;

public record CreateHumanAgentCommand(Guid OwnerUserId, CreateHumanAgentRequest Request) : IRequest<HumanAgentListItem>;

public class CreateHumanAgentCommandHandler : IRequestHandler<CreateHumanAgentCommand, HumanAgentListItem>
{
    private readonly AppDbContext _db;

    public CreateHumanAgentCommandHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<HumanAgentListItem> Handle(CreateHumanAgentCommand request, CancellationToken cancellationToken)
    {
        var agent = new HumanAgent
        {
            OwnerUserId = request.OwnerUserId,
            Name = request.Request.Name,
            Email = request.Request.Email,
            MaxConcurrentCalls = request.Request.MaxConcurrentCalls,
            Status = HumanAgentStatus.Offline,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.HumanAgents.Add(agent);
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
