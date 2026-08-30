using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediatR;
using backend.Data;

namespace backend.Modules.Configuration.Features.KnowledgeBases.DeleteDocument;

public record DeleteDocumentCommand(Guid DocumentId) : IRequest<bool>;

public class DeleteDocumentCommandHandler : IRequestHandler<DeleteDocumentCommand, bool>
{
    private readonly AppDbContext _db;

    public DeleteDocumentCommandHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<bool> Handle(DeleteDocumentCommand request, CancellationToken cancellationToken)
    {
        var doc = await _db.KnowledgeDocuments.FindAsync(new object[] { request.DocumentId }, cancellationToken);
        if (doc == null) return false;
        
        _db.KnowledgeDocuments.Remove(doc);
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
