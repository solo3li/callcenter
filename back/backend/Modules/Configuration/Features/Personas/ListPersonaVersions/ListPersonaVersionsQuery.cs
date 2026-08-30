using System;
using System.Collections.Generic;
using System.Linq;
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

namespace backend.Modules.Configuration.Features.Personas.ListPersonaVersions;

public record ListPersonaVersionsQuery(Guid PersonaId, Guid UserId) : IRequest<List<PersonaVersionDto>>;

public class ListPersonaVersionsQueryHandler : IRequestHandler<ListPersonaVersionsQuery, List<PersonaVersionDto>>
{
    private readonly AppDbContext _db;

    public ListPersonaVersionsQueryHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<PersonaVersionDto>> Handle(ListPersonaVersionsQuery request, CancellationToken cancellationToken)
    {
        var persona = await _db.Personas
            .FirstOrDefaultAsync(p => p.Id == request.PersonaId && p.UserId == request.UserId && p.IsActive, cancellationToken);

        if (persona is null)
            return new List<PersonaVersionDto>();

        var versions = await _db.PersonaVersions
            .Where(v => v.PersonaId == request.PersonaId)
            .OrderByDescending(v => v.VersionNumber)
            .ToListAsync(cancellationToken);

        return versions.Select(PersonaMapper.MapVersion).ToList();
    }
}
