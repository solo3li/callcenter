using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using backend.Data;
using backend.Modules.Identity.Dtos;
using backend.Modules.Billing.Dtos;
using backend.Modules.CallOperations.Dtos;
using backend.Modules.Configuration.Dtos;
using backend.Modules.Analytics.Dtos;

namespace backend.Modules.Identity.Features.Licenses.GetLicense;

public record GetLicenseQuery(Guid Id) : IRequest<LicenseDto?>;

public class GetLicenseQueryHandler : IRequestHandler<GetLicenseQuery, LicenseDto?>
{
    private readonly AppDbContext _db;

    public GetLicenseQueryHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<LicenseDto?> Handle(GetLicenseQuery request, CancellationToken cancellationToken)
    {
        var license = await _db.Licenses.FindAsync(new object[] { request.Id }, cancellationToken);
        return license == null ? null : LicenseMapper.Map(license);
    }
}
