using System;
using System.Collections.Generic;
using System.Linq;
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

namespace backend.Modules.Configuration.Features.Workflows.ListWorkflows;

public record ListWorkflowsQuery(Guid UserId) : IRequest<List<WorkflowListItem>>;

public class ListWorkflowsQueryHandler : IRequestHandler<ListWorkflowsQuery, List<WorkflowListItem>>
{
    private readonly AppDbContext _db;

    public ListWorkflowsQueryHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<WorkflowListItem>> Handle(ListWorkflowsQuery request, CancellationToken cancellationToken)
    {
        var workflows = await _db.Workflows
            .Where(w => w.UserId == request.UserId)
            .Include(w => w.Versions)
            .OrderByDescending(w => w.CreatedAt)
            .ToListAsync(cancellationToken);

        return workflows.Select(w => new WorkflowListItem(
            w.Id, w.Name, w.Description, w.IsActive,
            w.Versions.Count, w.CreatedAt, w.UpdatedAt)).ToList();
    }
}
