using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediatR;
using backend.Data;
using backend.Modules.Configuration.Models;

namespace backend.Modules.Configuration.Features.Workflows.UpdateWorkflow;

public record UpdateWorkflowCommand(Guid Id, string? Name, string? Description, bool? IsActive) : IRequest<Workflow?>;

public class UpdateWorkflowCommandHandler : IRequestHandler<UpdateWorkflowCommand, Workflow?>
{
    private readonly AppDbContext _db;

    public UpdateWorkflowCommandHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Workflow?> Handle(UpdateWorkflowCommand request, CancellationToken cancellationToken)
    {
        var workflow = await _db.Workflows.FindAsync(new object[] { request.Id }, cancellationToken);
        if (workflow == null) return null;

        if (request.Name != null) workflow.Name = request.Name;
        if (request.Description != null) workflow.Description = request.Description;
        if (request.IsActive.HasValue) workflow.IsActive = request.IsActive.Value;
        workflow.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);
        return workflow;
    }
}
