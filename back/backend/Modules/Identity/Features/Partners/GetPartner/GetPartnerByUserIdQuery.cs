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

public record GetPartnerByUserIdQuery(Guid UserId) : IRequest<PartnerDto?>;

public class GetPartnerByUserIdQueryHandler : IRequestHandler<GetPartnerByUserIdQuery, PartnerDto?>
{
    private readonly AppDbContext _db;

    public GetPartnerByUserIdQueryHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<PartnerDto?> Handle(GetPartnerByUserIdQuery request, CancellationToken cancellationToken)
    {
        var partner = await _db.Partners.FirstOrDefaultAsync(p => p.UserId == request.UserId, cancellationToken);
        return partner == null ? null : PartnerMapper.Map(partner);
    }
}
