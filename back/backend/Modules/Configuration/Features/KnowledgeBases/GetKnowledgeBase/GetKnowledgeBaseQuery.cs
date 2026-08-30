using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediatR;
using backend.Data;
using backend.Modules.Configuration.Models;

namespace backend.Modules.Configuration.Features.KnowledgeBases.GetKnowledgeBase;

public record GetKnowledgeBaseQuery(Guid Id) : IRequest<KnowledgeBase?>;

public class GetKnowledgeBaseQueryHandler : IRequestHandler<GetKnowledgeBaseQuery, KnowledgeBase?>
{
    private readonly AppDbContext _db;

    public GetKnowledgeBaseQueryHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<KnowledgeBase?> Handle(GetKnowledgeBaseQuery request, CancellationToken cancellationToken)
    {
        return await _db.KnowledgeBases
            .Include(k => k.Documents)
            .FirstOrDefaultAsync(k => k.Id == request.Id, cancellationToken);
    }
}
