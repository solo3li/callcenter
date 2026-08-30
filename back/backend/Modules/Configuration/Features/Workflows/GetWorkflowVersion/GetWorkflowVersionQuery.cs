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

namespace backend.Modules.Configuration.Features.Workflows.GetWorkflowVersion;

public record GetWorkflowVersionQuery(Guid VersionId) : IRequest<WorkflowVersionDto?>;

public class GetWorkflowVersionQueryHandler : IRequestHandler<GetWorkflowVersionQuery, WorkflowVersionDto?>
{
    private readonly AppDbContext _db;

    public GetWorkflowVersionQueryHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<WorkflowVersionDto?> Handle(GetWorkflowVersionQuery request, CancellationToken cancellationToken)
    {
        var v = await _db.WorkflowVersions.FindAsync(new object[] { request.VersionId }, cancellationToken);
        if (v == null) return null;
        
        return new WorkflowVersionDto(
            v.Id, v.WorkflowId, v.VersionNumber,
            v.DefinitionJson, v.IsPublished, v.CreatedAt);
    }
}
