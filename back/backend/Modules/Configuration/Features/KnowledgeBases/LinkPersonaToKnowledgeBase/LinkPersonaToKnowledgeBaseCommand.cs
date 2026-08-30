using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediatR;
using backend.Data;
using backend.Modules.Configuration.Models;

namespace backend.Modules.Configuration.Features.KnowledgeBases.LinkPersonaToKnowledgeBase;

public record LinkPersonaToKnowledgeBaseCommand(Guid PersonaId, Guid KnowledgeBaseId) : IRequest<bool>;

public class LinkPersonaToKnowledgeBaseCommandHandler : IRequestHandler<LinkPersonaToKnowledgeBaseCommand, bool>
{
    private readonly AppDbContext _db;

    public LinkPersonaToKnowledgeBaseCommandHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<bool> Handle(LinkPersonaToKnowledgeBaseCommand request, CancellationToken cancellationToken)
    {
        var exists = await _db.PersonaKnowledgeBases
            .AnyAsync(pk => pk.PersonaId == request.PersonaId && pk.KnowledgeBaseId == request.KnowledgeBaseId, cancellationToken);

        if (exists) return false;

        _db.PersonaKnowledgeBases.Add(new PersonaKnowledgeBase
        {
            PersonaId = request.PersonaId,
            KnowledgeBaseId = request.KnowledgeBaseId,
            CreatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
