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
using backend.Models.Enums;

namespace backend.Modules.Identity.Features.Partners.GetProvisionStatus;

public record GetProvisionStatusQuery(Guid PartnerId, string ExternalCustomerId) : IRequest<ProvisionResponse>;

public class GetProvisionStatusQueryHandler : IRequestHandler<GetProvisionStatusQuery, ProvisionResponse>
{
    private readonly AppDbContext _db;

    public GetProvisionStatusQueryHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<ProvisionResponse> Handle(GetProvisionStatusQuery request, CancellationToken cancellationToken)
    {
        var ec = await _db.PartnerExternalCustomers
            .FirstOrDefaultAsync(e => e.PartnerId == request.PartnerId && e.ExternalCustomerId == request.ExternalCustomerId, cancellationToken);

        if (ec == null)
            throw new InvalidOperationException("Customer not found");

        var rel = await _db.PartnerRelationships
            .FirstOrDefaultAsync(r => r.PartnerId == request.PartnerId && r.CustomerUserId == ec.PlatformUserId, cancellationToken);

        var license = await _db.Licenses
            .FirstOrDefaultAsync(l => l.UserId == ec.PlatformUserId && l.PartnerId == request.PartnerId, cancellationToken);

        var key = await _db.ApiKeys
            .FirstOrDefaultAsync(k => k.UserId == ec.PlatformUserId && k.Status == ApiKeyStatus.Active, cancellationToken);

        return new ProvisionResponse(
            ec.PlatformUserId,
            rel?.Id ?? Guid.Empty,
            license?.Id,
            key?.KeyPrefix ?? string.Empty);
    }
}
