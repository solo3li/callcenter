using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediatR;
using backend.Data;
using backend.Dtos;
using backend.Modules.Configuration.Models;

namespace backend.Modules.Configuration.Features.Personas.CreatePersonaVersion;

public record CreatePersonaVersionCommand(Guid PersonaId, Guid UserId, string? SystemPrompt, string? ConfigurationJson) : IRequest<PersonaVersionDto>;

public class CreatePersonaVersionCommandHandler : IRequestHandler<CreatePersonaVersionCommand, PersonaVersionDto>
{
    private readonly AppDbContext _db;

    public CreatePersonaVersionCommandHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<PersonaVersionDto> Handle(CreatePersonaVersionCommand request, CancellationToken cancellationToken)
    {
        var persona = await _db.Personas
            .FirstOrDefaultAsync(p => p.Id == request.PersonaId && p.UserId == request.UserId && p.IsActive, cancellationToken)
            ?? throw new InvalidOperationException("Persona not found.");

        var maxVersion = await _db.PersonaVersions
            .Where(v => v.PersonaId == request.PersonaId)
            .MaxAsync(v => (int?)v.VersionNumber, cancellationToken) ?? 0;

        var systemPrompt = request.SystemPrompt;
        var configurationJson = request.ConfigurationJson ?? "{}";

        if (string.IsNullOrWhiteSpace(systemPrompt))
        {
            var latest = await _db.PersonaVersions
                .Where(v => v.PersonaId == request.PersonaId)
                .OrderByDescending(v => v.VersionNumber)
                .FirstOrDefaultAsync(cancellationToken);

            systemPrompt = latest?.SystemPrompt ?? string.Empty;
            configurationJson = latest?.ConfigurationJson ?? "{}";
        }

        var version = new PersonaVersion
        {
            PersonaId = request.PersonaId,
            VersionNumber = maxVersion + 1,
            SystemPrompt = systemPrompt,
            ConfigurationJson = configurationJson,
            IsPublished = false,
            CreatedAt = DateTime.UtcNow
        };

        _db.PersonaVersions.Add(version);
        await _db.SaveChangesAsync(cancellationToken);

        return PersonaMapper.MapVersion(version);
    }
}
