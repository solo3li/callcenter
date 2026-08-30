using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediatR;
using backend.Data;
using backend.Modules.Configuration.Models;

namespace backend.Modules.Configuration.Features.Workflows.CreateWorkflow;

public record CreateWorkflowCommand(Guid UserId, string Name, string? Description) : IRequest<Workflow>;

public class CreateWorkflowCommandHandler : IRequestHandler<CreateWorkflowCommand, Workflow>
{
    private readonly AppDbContext _db;

    public CreateWorkflowCommandHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Workflow> Handle(CreateWorkflowCommand request, CancellationToken cancellationToken)
    {
        var workflow = new Workflow
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            Name = request.Name,
            Description = request.Description,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.Workflows.Add(workflow);
        await _db.SaveChangesAsync(cancellationToken);
        return workflow;
    }
}
