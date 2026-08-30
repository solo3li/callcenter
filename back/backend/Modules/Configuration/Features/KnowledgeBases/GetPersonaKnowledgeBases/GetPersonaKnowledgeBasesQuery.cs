using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediatR;
using backend.Data;
using backend.Dtos;

namespace backend.Modules.Configuration.Features.KnowledgeBases.GetPersonaKnowledgeBases;

public record GetPersonaKnowledgeBasesQuery(Guid PersonaId) : IRequest<List<KnowledgeBaseListItem>>;

public class GetPersonaKnowledgeBasesQueryHandler : IRequestHandler<GetPersonaKnowledgeBasesQuery, List<KnowledgeBaseListItem>>
{
    private readonly AppDbContext _db;

    public GetPersonaKnowledgeBasesQueryHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<KnowledgeBaseListItem>> Handle(GetPersonaKnowledgeBasesQuery request, CancellationToken cancellationToken)
    {
        var kbIds = await _db.PersonaKnowledgeBases
            .Where(pk => pk.PersonaId == request.PersonaId)
            .Select(pk => pk.KnowledgeBaseId)
            .ToListAsync(cancellationToken);

        var kbs = await _db.KnowledgeBases
            .Where(k => kbIds.Contains(k.Id))
            .Include(k => k.Documents)
            .ToListAsync(cancellationToken);

        return kbs.Select(k => new KnowledgeBaseListItem(
            k.Id, k.Name, k.Description, k.IsActive,
            k.Documents.Count, k.CreatedAt, k.UpdatedAt)).ToList();
    }
}
