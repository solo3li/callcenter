using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediatR;
using backend.Data;
using backend.Dtos;

namespace backend.Modules.Identity.Features.Licenses.ListLicenses;

public record ListLicensesQuery(Guid? UserId = null, Guid? PartnerId = null) : IRequest<List<LicenseDto>>;

public class ListLicensesQueryHandler : IRequestHandler<ListLicensesQuery, List<LicenseDto>>
{
    private readonly AppDbContext _db;

    public ListLicensesQueryHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<LicenseDto>> Handle(ListLicensesQuery request, CancellationToken cancellationToken)
    {
        var query = _db.Licenses.AsQueryable();
        if (request.UserId.HasValue)
            query = query.Where(l => l.UserId == request.UserId.Value);
        if (request.PartnerId.HasValue)
            query = query.Where(l => l.PartnerId == request.PartnerId.Value);

        var licenses = await query.OrderByDescending(l => l.CreatedAt).ToListAsync(cancellationToken);
        return licenses.Select(LicenseMapper.Map).ToList();
    }
}
