using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediatR;
using backend.Data;

namespace backend.Modules.Configuration.Features.Workflows.DeleteWorkflow;

public record DeleteWorkflowCommand(Guid Id) : IRequest<bool>;

public class DeleteWorkflowCommandHandler : IRequestHandler<DeleteWorkflowCommand, bool>
{
    private readonly AppDbContext _db;

    public DeleteWorkflowCommandHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<bool> Handle(DeleteWorkflowCommand request, CancellationToken cancellationToken)
    {
        var workflow = await _db.Workflows.FindAsync(new object[] { request.Id }, cancellationToken);
        if (workflow == null) return false;
        
        _db.Workflows.Remove(workflow);
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
