using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediatR;
using backend.Data;
using backend.Dtos;

namespace backend.Modules.Identity.Features.Partners.ListPartners;

public record ListPartnersQuery() : IRequest<List<PartnerDto>>;

public class ListPartnersQueryHandler : IRequestHandler<ListPartnersQuery, List<PartnerDto>>
{
    private readonly AppDbContext _db;

    public ListPartnersQueryHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<PartnerDto>> Handle(ListPartnersQuery request, CancellationToken cancellationToken)
    {
        var partners = await _db.Partners
            .Where(p => p.IsActive)
            .OrderBy(p => p.OrganizationName)
            .ToListAsync(cancellationToken);
        return partners.Select(PartnerMapper.Map).ToList();
    }
}
