using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using backend.Data;
using backend.Dtos;

namespace backend.Modules.Identity.Features.Partners.GetPartnerRelationship;

public record GetPartnerRelationshipQuery(Guid Id) : IRequest<PartnerRelationshipDto?>;

public class GetPartnerRelationshipQueryHandler : IRequestHandler<GetPartnerRelationshipQuery, PartnerRelationshipDto?>
{
    private readonly AppDbContext _db;

    public GetPartnerRelationshipQueryHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<PartnerRelationshipDto?> Handle(GetPartnerRelationshipQuery request, CancellationToken cancellationToken)
    {
        var rel = await _db.PartnerRelationships.FindAsync(new object[] { request.Id }, cancellationToken);
        return rel == null ? null : PartnerMapper.MapRelation(rel);
    }
}
