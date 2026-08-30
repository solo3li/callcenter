using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediatR;
using backend.Data;
using backend.Modules.Identity.Dtos;
using backend.Modules.Billing.Dtos;
using backend.Modules.CallOperations.Dtos;
using backend.Modules.Configuration.Dtos;
using backend.Modules.Analytics.Dtos;

namespace backend.Modules.Identity.Features.Partners.GetPartner;

public record GetPartnerQuery(Guid Id) : IRequest<PartnerDto?>;

public class GetPartnerQueryHandler : IRequestHandler<GetPartnerQuery, PartnerDto?>
{
    private readonly AppDbContext _db;

    public GetPartnerQueryHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<PartnerDto?> Handle(GetPartnerQuery request, CancellationToken cancellationToken)
    {
        var partner = await _db.Partners.FindAsync(new object[] { request.Id }, cancellationToken);
        return partner == null ? null : PartnerMapper.Map(partner);
    }
}
