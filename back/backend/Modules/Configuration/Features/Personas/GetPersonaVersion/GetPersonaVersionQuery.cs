using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediatR;
using backend.Data;
using backend.Dtos;

namespace backend.Modules.Configuration.Features.Personas.GetPersonaVersion;

public record GetPersonaVersionQuery(Guid PersonaId, Guid VersionId, Guid UserId) : IRequest<PersonaVersionDto?>;

public class GetPersonaVersionQueryHandler : IRequestHandler<GetPersonaVersionQuery, PersonaVersionDto?>
{
    private readonly AppDbContext _db;

    public GetPersonaVersionQueryHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<PersonaVersionDto?> Handle(GetPersonaVersionQuery request, CancellationToken cancellationToken)
    {
        var persona = await _db.Personas
            .FirstOrDefaultAsync(p => p.Id == request.PersonaId && p.UserId == request.UserId && p.IsActive, cancellationToken);

        if (persona is null)
            return null;

        var version = await _db.PersonaVersions
            .FirstOrDefaultAsync(v => v.Id == request.VersionId && v.PersonaId == request.PersonaId, cancellationToken);

        return version is null ? null : PersonaMapper.MapVersion(version);
    }
}
