using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediatR;
using backend.Data;

namespace backend.Modules.Configuration.Features.KnowledgeBases.DeleteChunk;

public record DeleteChunkCommand(Guid ChunkId) : IRequest<bool>;

public class DeleteChunkCommandHandler : IRequestHandler<DeleteChunkCommand, bool>
{
    private readonly AppDbContext _db;

    public DeleteChunkCommandHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<bool> Handle(DeleteChunkCommand request, CancellationToken cancellationToken)
    {
        var chunk = await _db.KnowledgeChunks.FindAsync(new object[] { request.ChunkId }, cancellationToken);
        if (chunk == null) return false;
        
        _db.KnowledgeChunks.Remove(chunk);
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
