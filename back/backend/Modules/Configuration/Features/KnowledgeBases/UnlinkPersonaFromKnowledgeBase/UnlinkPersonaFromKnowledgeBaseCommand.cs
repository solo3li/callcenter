using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediatR;
using backend.Data;

namespace backend.Modules.Configuration.Features.KnowledgeBases.UnlinkPersonaFromKnowledgeBase;

public record UnlinkPersonaFromKnowledgeBaseCommand(Guid PersonaId, Guid KnowledgeBaseId) : IRequest<bool>;

public class UnlinkPersonaFromKnowledgeBaseCommandHandler : IRequestHandler<UnlinkPersonaFromKnowledgeBaseCommand, bool>
{
    private readonly AppDbContext _db;

    public UnlinkPersonaFromKnowledgeBaseCommandHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<bool> Handle(UnlinkPersonaFromKnowledgeBaseCommand request, CancellationToken cancellationToken)
    {
        var pk = await _db.PersonaKnowledgeBases
            .FirstOrDefaultAsync(p => p.PersonaId == request.PersonaId && p.KnowledgeBaseId == request.KnowledgeBaseId, cancellationToken);

        if (pk == null) return false;

        _db.PersonaKnowledgeBases.Remove(pk);
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
