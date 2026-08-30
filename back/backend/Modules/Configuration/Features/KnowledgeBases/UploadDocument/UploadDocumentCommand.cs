using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediatR;
using backend.Data;
using backend.Dtos;
using backend.Modules.Configuration.Models;

namespace backend.Modules.Configuration.Features.KnowledgeBases.UploadDocument;

public record UploadDocumentCommand(Guid KnowledgeBaseId, string Name, string SourceUri, string ContentType, string Content, string? MetadataJson) : IRequest<KnowledgeDocumentDto>;

public class UploadDocumentCommandHandler : IRequestHandler<UploadDocumentCommand, KnowledgeDocumentDto>
{
    private readonly AppDbContext _db;

    public UploadDocumentCommandHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<KnowledgeDocumentDto> Handle(UploadDocumentCommand request, CancellationToken cancellationToken)
    {
        var doc = new KnowledgeDocument
        {
            Id = Guid.NewGuid(),
            KnowledgeBaseId = request.KnowledgeBaseId,
            Name = request.Name,
            SourceUri = request.SourceUri,
            ContentType = request.ContentType,
            MetadataJson = request.MetadataJson,
            Status = "processing",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.KnowledgeDocuments.Add(doc);

        var chunkSize = 1000;
        var content = request.Content;
        var chunkIndex = 0;

        for (var i = 0; i < content.Length; i += chunkSize)
        {
            var chunkContent = content.Substring(i, Math.Min(chunkSize, content.Length - i));
            var chunk = new KnowledgeChunk
            {
                Id = Guid.NewGuid(),
                KnowledgeDocumentId = doc.Id,
                ChunkIndex = chunkIndex,
                Content = chunkContent,
                CreatedAt = DateTime.UtcNow
            };
            _db.KnowledgeChunks.Add(chunk);
            chunkIndex++;
        }

        doc.Status = "ready";
        await _db.SaveChangesAsync(cancellationToken);
        
        return new KnowledgeDocumentDto(
            doc.Id, doc.KnowledgeBaseId, doc.Name, doc.SourceUri,
            doc.ContentType, doc.MetadataJson, doc.Status,
            chunkIndex, doc.CreatedAt, doc.UpdatedAt);
    }
}
