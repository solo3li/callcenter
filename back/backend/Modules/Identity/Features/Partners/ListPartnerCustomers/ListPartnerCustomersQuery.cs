using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediatR;
using backend.Data;
using backend.Dtos;

namespace backend.Modules.Identity.Features.Partners.ListPartnerCustomers;

public record ListPartnerCustomersQuery(Guid PartnerId) : IRequest<List<PartnerRelationshipDto>>;

public class ListPartnerCustomersQueryHandler : IRequestHandler<ListPartnerCustomersQuery, List<PartnerRelationshipDto>>
{
    private readonly AppDbContext _db;

    public ListPartnerCustomersQueryHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<PartnerRelationshipDto>> Handle(ListPartnerCustomersQuery request, CancellationToken cancellationToken)
    {
        var relationships = await _db.PartnerRelationships
            .Where(r => r.PartnerId == request.PartnerId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
        return relationships.Select(PartnerMapper.MapRelation).ToList();
    }
}
