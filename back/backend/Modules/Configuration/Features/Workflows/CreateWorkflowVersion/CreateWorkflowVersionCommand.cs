using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediatR;
using backend.Data;
using backend.Dtos;
using backend.Modules.Configuration.Models;

namespace backend.Modules.Configuration.Features.Workflows.CreateWorkflowVersion;

public record CreateWorkflowVersionCommand(Guid WorkflowId, string DefinitionJson) : IRequest<WorkflowVersionDto>;

public class CreateWorkflowVersionCommandHandler : IRequestHandler<CreateWorkflowVersionCommand, WorkflowVersionDto>
{
    private readonly AppDbContext _db;

    public CreateWorkflowVersionCommandHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<WorkflowVersionDto> Handle(CreateWorkflowVersionCommand request, CancellationToken cancellationToken)
    {
        var maxVersion = await _db.WorkflowVersions
            .Where(v => v.WorkflowId == request.WorkflowId)
            .MaxAsync(v => (int?)v.VersionNumber, cancellationToken) ?? 0;

        var version = new WorkflowVersion
        {
            Id = Guid.NewGuid(),
            WorkflowId = request.WorkflowId,
            VersionNumber = maxVersion + 1,
            DefinitionJson = request.DefinitionJson,
            IsPublished = true,
            CreatedAt = DateTime.UtcNow
        };

        _db.WorkflowVersions.Add(version);
        await _db.SaveChangesAsync(cancellationToken);
        
        return new WorkflowVersionDto(
            version.Id, version.WorkflowId, version.VersionNumber,
            version.DefinitionJson, version.IsPublished, version.CreatedAt);
    }
}
