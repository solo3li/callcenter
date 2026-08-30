using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediatR;
using backend.Data;

namespace backend.Modules.Configuration.Features.KnowledgeBases.DeleteKnowledgeBase;

public record DeleteKnowledgeBaseCommand(Guid Id) : IRequest<bool>;

public class DeleteKnowledgeBaseCommandHandler : IRequestHandler<DeleteKnowledgeBaseCommand, bool>
{
    private readonly AppDbContext _db;

    public DeleteKnowledgeBaseCommandHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<bool> Handle(DeleteKnowledgeBaseCommand request, CancellationToken cancellationToken)
    {
        var kb = await _db.KnowledgeBases.FindAsync(new object[] { request.Id }, cancellationToken);
        if (kb == null) return false;
        
        _db.KnowledgeBases.Remove(kb);
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
