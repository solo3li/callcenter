using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediatR;
using backend.Data;
using backend.Modules.Configuration.Models;

namespace backend.Modules.Configuration.Features.Workflows.GetWorkflow;

public record GetWorkflowQuery(Guid Id) : IRequest<Workflow?>;

public class GetWorkflowQueryHandler : IRequestHandler<GetWorkflowQuery, Workflow?>
{
    private readonly AppDbContext _db;

    public GetWorkflowQueryHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Workflow?> Handle(GetWorkflowQuery request, CancellationToken cancellationToken)
    {
        return await _db.Workflows
            .Include(w => w.Versions)
            .FirstOrDefaultAsync(w => w.Id == request.Id, cancellationToken);
    }
}
