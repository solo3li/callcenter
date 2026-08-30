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
using backend.Services;

namespace backend.Modules.Configuration.Features.KnowledgeBases.SearchKnowledgeBases;

public record SearchKnowledgeBasesQuery(Guid KnowledgeBaseId, string Query, int TopK = 5) : IRequest<List<SearchResult>>;

public class SearchKnowledgeBasesQueryHandler : IRequestHandler<SearchKnowledgeBasesQuery, List<SearchResult>>
{
    private readonly AppDbContext _db;
    private readonly EmbeddingService _embeddingService;

    public SearchKnowledgeBasesQueryHandler(AppDbContext db, EmbeddingService embeddingService)
    {
        _db = db;
        _embeddingService = embeddingService;
    }

    public async Task<List<SearchResult>> Handle(SearchKnowledgeBasesQuery request, CancellationToken cancellationToken)
    {
        var hasEmbeddings = await _db.KnowledgeChunks
            .AnyAsync(c => c.KnowledgeDocument.KnowledgeBaseId == request.KnowledgeBaseId && c.Embedding != null, cancellationToken);

        if (hasEmbeddings)
        {
            var embedding = await _embeddingService.GenerateEmbeddingAsync(request.Query);
            var vectorStr = _embeddingService.FormatVectorLiteral(embedding);

            var sql = $@"
                SELECT c.""id"", c.""knowledge_document_id"", d.""name"", c.""content"", c.""chunk_index"",
                       c.""embedding"" <=> '{vectorStr}'::vector AS similarity
                FROM knowledge_chunks c
                JOIN knowledge_documents d ON c.""knowledge_document_id"" = d.""id""
                WHERE d.""knowledge_base_id"" = '{request.KnowledgeBaseId}'
                ORDER BY similarity
                LIMIT {request.TopK}";

            var results = await _db.Database.SqlQueryRaw<SearchResultRaw>(sql).ToListAsync(cancellationToken);
            return results.Select(r => new SearchResult(r.id, r.knowledge_document_id, r.name, r.content, r.chunk_index, (float?)r.similarity)).ToList();
        }

        var chunks = await _db.KnowledgeChunks
            .Where(c => c.KnowledgeDocument.KnowledgeBaseId == request.KnowledgeBaseId)
            .Where(c => EF.Functions.ILike(c.Content, $"%{request.Query}%"))
            .Include(c => c.KnowledgeDocument)
            .Take(request.TopK)
            .ToListAsync(cancellationToken);

        return chunks.Select(c => new SearchResult(
            c.Id,
            c.KnowledgeDocumentId,
            c.KnowledgeDocument.Name,
            c.Content,
            c.ChunkIndex,
            null)).ToList();
    }
}

public class SearchResultRaw
{
    public Guid id { get; set; }
    public Guid knowledge_document_id { get; set; }
    public string name { get; set; } = "";
    public string content { get; set; } = "";
    public int chunk_index { get; set; }
    public double? similarity { get; set; }
}
