using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediatR;
using backend.Data;
using backend.Dtos;

namespace backend.Modules.Configuration.Features.Workflows.ListWorkflowVersions;

public record ListWorkflowVersionsQuery(Guid WorkflowId) : IRequest<List<WorkflowVersionDto>>;

public class ListWorkflowVersionsQueryHandler : IRequestHandler<ListWorkflowVersionsQuery, List<WorkflowVersionDto>>
{
    private readonly AppDbContext _db;

    public ListWorkflowVersionsQueryHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<WorkflowVersionDto>> Handle(ListWorkflowVersionsQuery request, CancellationToken cancellationToken)
    {
        var versions = await _db.WorkflowVersions
            .Where(v => v.WorkflowId == request.WorkflowId)
            .OrderByDescending(v => v.VersionNumber)
            .ToListAsync(cancellationToken);

        return versions.Select(v => new WorkflowVersionDto(
            v.Id, v.WorkflowId, v.VersionNumber,
            v.DefinitionJson, v.IsPublished, v.CreatedAt)).ToList();
    }
}
