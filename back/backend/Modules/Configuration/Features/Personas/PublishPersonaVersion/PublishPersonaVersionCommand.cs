using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediatR;
using backend.Data;
using backend.Dtos;

namespace backend.Modules.Configuration.Features.Personas.PublishPersonaVersion;

public record PublishPersonaVersionCommand(Guid PersonaId, Guid VersionId, Guid UserId) : IRequest<PersonaVersionDto?>;

public class PublishPersonaVersionCommandHandler : IRequestHandler<PublishPersonaVersionCommand, PersonaVersionDto?>
{
    private readonly AppDbContext _db;

    public PublishPersonaVersionCommandHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<PersonaVersionDto?> Handle(PublishPersonaVersionCommand request, CancellationToken cancellationToken)
    {
        var persona = await _db.Personas
            .FirstOrDefaultAsync(p => p.Id == request.PersonaId && p.UserId == request.UserId && p.IsActive, cancellationToken);

        if (persona is null)
            return null;

        var version = await _db.PersonaVersions
            .FirstOrDefaultAsync(v => v.Id == request.VersionId && v.PersonaId == request.PersonaId, cancellationToken);

        if (version is null)
            return null;

        var currentlyPublished = await _db.PersonaVersions
            .Where(v => v.PersonaId == request.PersonaId && v.IsPublished)
            .ToListAsync(cancellationToken);

        foreach (var published in currentlyPublished)
        {
            published.IsPublished = false;
        }

        version.IsPublished = true;
        await _db.SaveChangesAsync(cancellationToken);

        return PersonaMapper.MapVersion(version);
    }
}
