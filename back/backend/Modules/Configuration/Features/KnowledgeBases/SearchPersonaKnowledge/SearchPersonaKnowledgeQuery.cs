using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediatR;
using backend.Data;
using backend.Dtos;
using backend.Services;

namespace backend.Modules.Configuration.Features.KnowledgeBases.SearchPersonaKnowledge;

public record SearchPersonaKnowledgeQuery(Guid PersonaId, string Query, int TopK = 4) : IRequest<List<SearchResult>>;

public class SearchPersonaKnowledgeQueryHandler : IRequestHandler<SearchPersonaKnowledgeQuery, List<SearchResult>>
{
    private readonly AppDbContext _db;
    private readonly IMediator _mediator;

    public SearchPersonaKnowledgeQueryHandler(AppDbContext db, IMediator mediator)
    {
        _db = db;
        _mediator = mediator;
    }

    public async Task<List<SearchResult>> Handle(SearchPersonaKnowledgeQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var kbIds = await _db.PersonaKnowledgeBases
                .Where(pk => pk.PersonaId == request.PersonaId)
                .Select(pk => pk.KnowledgeBaseId)
                .ToListAsync(cancellationToken);

            if (kbIds.Count == 0) return new List<SearchResult>();

            var merged = new List<SearchResult>();
            foreach (var kbId in kbIds)
            {
                var kbResults = await _mediator.Send(new backend.Modules.Configuration.Features.KnowledgeBases.SearchKnowledgeBases.SearchKnowledgeBasesQuery(kbId, request.Query, request.TopK), cancellationToken);
                merged.AddRange(kbResults);
            }

            return merged
                .OrderBy(r => r.Score ?? float.MaxValue) // cosine distance: lower is better
                .Take(request.TopK)
                .ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[KB] Persona knowledge search failed: {ex.Message}");
            return new List<SearchResult>();
        }
    }
}
