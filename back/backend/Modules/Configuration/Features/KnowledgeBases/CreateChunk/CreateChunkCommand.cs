using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediatR;
using backend.Data;
using backend.Modules.Configuration.Models;

namespace backend.Modules.Configuration.Features.KnowledgeBases.CreateChunk;

public record CreateChunkCommand(Guid DocumentId, int ChunkIndex, string Content, string? MetadataJson) : IRequest<KnowledgeChunk>;

public class CreateChunkCommandHandler : IRequestHandler<CreateChunkCommand, KnowledgeChunk>
{
    private readonly AppDbContext _db;

    public CreateChunkCommandHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<KnowledgeChunk> Handle(CreateChunkCommand request, CancellationToken cancellationToken)
    {
        var chunk = new KnowledgeChunk
        {
            Id = Guid.NewGuid(),
            KnowledgeDocumentId = request.DocumentId,
            ChunkIndex = request.ChunkIndex,
            Content = request.Content,
            MetadataJson = request.MetadataJson,
            CreatedAt = DateTime.UtcNow
        };

        _db.KnowledgeChunks.Add(chunk);
        await _db.SaveChangesAsync(cancellationToken);
        return chunk;
    }
}
