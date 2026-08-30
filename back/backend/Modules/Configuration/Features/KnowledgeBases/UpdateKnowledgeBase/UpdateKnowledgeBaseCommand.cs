using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediatR;
using backend.Data;
using backend.Modules.Configuration.Models;

namespace backend.Modules.Configuration.Features.KnowledgeBases.UpdateKnowledgeBase;

public record UpdateKnowledgeBaseCommand(Guid Id, string? Name, string? Description, bool? IsActive) : IRequest<KnowledgeBase?>;

public class UpdateKnowledgeBaseCommandHandler : IRequestHandler<UpdateKnowledgeBaseCommand, KnowledgeBase?>
{
    private readonly AppDbContext _db;

    public UpdateKnowledgeBaseCommandHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<KnowledgeBase?> Handle(UpdateKnowledgeBaseCommand request, CancellationToken cancellationToken)
    {
        var kb = await _db.KnowledgeBases.FindAsync(new object[] { request.Id }, cancellationToken);
        if (kb == null) return null;

        if (request.Name != null) kb.Name = request.Name;
        if (request.Description != null) kb.Description = request.Description;
        if (request.IsActive.HasValue) kb.IsActive = request.IsActive.Value;
        kb.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);
        return kb;
    }
}
