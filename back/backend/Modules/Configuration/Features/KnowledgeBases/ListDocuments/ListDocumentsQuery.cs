using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediatR;
using backend.Data;
using backend.Dtos;

namespace backend.Modules.Configuration.Features.KnowledgeBases.ListDocuments;

public record ListDocumentsQuery(Guid KnowledgeBaseId) : IRequest<List<KnowledgeDocumentDto>>;

public class ListDocumentsQueryHandler : IRequestHandler<ListDocumentsQuery, List<KnowledgeDocumentDto>>
{
    private readonly AppDbContext _db;

    public ListDocumentsQueryHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<KnowledgeDocumentDto>> Handle(ListDocumentsQuery request, CancellationToken cancellationToken)
    {
        var docs = await _db.KnowledgeDocuments
            .Where(d => d.KnowledgeBaseId == request.KnowledgeBaseId)
            .Include(d => d.Chunks)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync(cancellationToken);

        return docs.Select(d => new KnowledgeDocumentDto(
            d.Id, d.KnowledgeBaseId, d.Name, d.SourceUri,
            d.ContentType, d.MetadataJson, d.Status,
            d.Chunks.Count, d.CreatedAt, d.UpdatedAt)).ToList();
    }
}
