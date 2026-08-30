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

namespace backend.Modules.Configuration.Features.KnowledgeBases.GetDocument;

public record GetDocumentQuery(Guid DocumentId) : IRequest<KnowledgeDocumentDto?>;

public class GetDocumentQueryHandler : IRequestHandler<GetDocumentQuery, KnowledgeDocumentDto?>
{
    private readonly AppDbContext _db;

    public GetDocumentQueryHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<KnowledgeDocumentDto?> Handle(GetDocumentQuery request, CancellationToken cancellationToken)
    {
        var d = await _db.KnowledgeDocuments
            .Include(d => d.Chunks)
            .FirstOrDefaultAsync(d => d.Id == request.DocumentId, cancellationToken);

        if (d == null) return null;
        
        return new KnowledgeDocumentDto(
            d.Id, d.KnowledgeBaseId, d.Name, d.SourceUri,
            d.ContentType, d.MetadataJson, d.Status,
            d.Chunks.Count, d.CreatedAt, d.UpdatedAt);
    }
}
