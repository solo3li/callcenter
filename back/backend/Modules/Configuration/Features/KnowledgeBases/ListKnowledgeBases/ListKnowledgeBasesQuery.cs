using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediatR;
using backend.Data;
using backend.Dtos;

namespace backend.Modules.Configuration.Features.KnowledgeBases.ListKnowledgeBases;

public record ListKnowledgeBasesQuery(Guid UserId) : IRequest<List<KnowledgeBaseListItem>>;

public class ListKnowledgeBasesQueryHandler : IRequestHandler<ListKnowledgeBasesQuery, List<KnowledgeBaseListItem>>
{
    private readonly AppDbContext _db;

    public ListKnowledgeBasesQueryHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<KnowledgeBaseListItem>> Handle(ListKnowledgeBasesQuery request, CancellationToken cancellationToken)
    {
        var kbs = await _db.KnowledgeBases
            .Where(k => k.UserId == request.UserId)
            .Include(k => k.Documents)
            .OrderByDescending(k => k.CreatedAt)
            .ToListAsync(cancellationToken);

        return kbs.Select(k => new KnowledgeBaseListItem(
            k.Id, k.Name, k.Description, k.IsActive,
            k.Documents.Count, k.CreatedAt, k.UpdatedAt)).ToList();
    }
}
