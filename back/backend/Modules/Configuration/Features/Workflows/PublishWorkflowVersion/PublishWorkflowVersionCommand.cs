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

namespace backend.Modules.Configuration.Features.Workflows.PublishWorkflowVersion;

public record PublishWorkflowVersionCommand(Guid VersionId) : IRequest<WorkflowVersionDto?>;

public class PublishWorkflowVersionCommandHandler : IRequestHandler<PublishWorkflowVersionCommand, WorkflowVersionDto?>
{
    private readonly AppDbContext _db;

    public PublishWorkflowVersionCommandHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<WorkflowVersionDto?> Handle(PublishWorkflowVersionCommand request, CancellationToken cancellationToken)
    {
        var version = await _db.WorkflowVersions.FindAsync(new object[] { request.VersionId }, cancellationToken);
        if (version == null) return null;

        var workflow = await _db.Workflows
            .Include(w => w.Versions)
            .FirstOrDefaultAsync(w => w.Id == version.WorkflowId, cancellationToken);

        if (workflow != null)
        {
            foreach (var v in workflow.Versions)
                v.IsPublished = false;
        }

        version.IsPublished = true;
        await _db.SaveChangesAsync(cancellationToken);

        return new WorkflowVersionDto(
            version.Id, version.WorkflowId, version.VersionNumber,
            version.DefinitionJson, version.IsPublished, version.CreatedAt);
    }
}
