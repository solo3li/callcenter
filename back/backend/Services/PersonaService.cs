using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Dtos;
using backend.Models.Domain;

namespace backend.Services;

public class PersonaService
{
    private readonly AppDbContext _db;

    public PersonaService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<PersonaListItem>> ListAsync(Guid userId)
    {
        return await _db.Personas
            .Where(p => p.UserId == userId && p.IsActive)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new PersonaListItem(
                p.Id,
                p.Name,
                p.Description,
                p.IsActive,
                p.CreatedAt,
                p.UpdatedAt
            ))
            .ToListAsync();
    }

    public async Task<PersonaListItem?> GetByIdAsync(Guid id, Guid userId)
    {
        return await _db.Personas
            .Where(p => p.Id == id && p.UserId == userId && p.IsActive)
            .Select(p => new PersonaListItem(
                p.Id,
                p.Name,
                p.Description,
                p.IsActive,
                p.CreatedAt,
                p.UpdatedAt
            ))
            .FirstOrDefaultAsync();
    }

    public async Task<PersonaListItem> CreateAsync(Guid userId, CreatePersonaRequest request)
    {
        var persona = new Persona
        {
            UserId = userId,
            Name = request.Name,
            Description = request.Description,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.Personas.Add(persona);
        await _db.SaveChangesAsync();

        return new PersonaListItem(
            persona.Id,
            persona.Name,
            persona.Description,
            persona.IsActive,
            persona.CreatedAt,
            persona.UpdatedAt
        );
    }

    public async Task<PersonaListItem?> UpdateAsync(Guid id, Guid userId, UpdatePersonaRequest request)
    {
        var persona = await _db.Personas
            .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId && p.IsActive);

        if (persona is null)
            return null;

        persona.Name = request.Name;
        persona.Description = request.Description;
        persona.IsActive = request.IsActive;
        persona.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return new PersonaListItem(
            persona.Id,
            persona.Name,
            persona.Description,
            persona.IsActive,
            persona.CreatedAt,
            persona.UpdatedAt
        );
    }

    public async Task<bool> DeleteAsync(Guid id, Guid userId)
    {
        var persona = await _db.Personas
            .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId && p.IsActive);

        if (persona is null)
            return false;

        persona.IsActive = false;
        persona.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return true;
    }

    public async Task<List<PersonaVersionDto>> ListVersionsAsync(Guid personaId, Guid userId)
    {
        var persona = await _db.Personas
            .FirstOrDefaultAsync(p => p.Id == personaId && p.UserId == userId && p.IsActive);

        if (persona is null)
            return new List<PersonaVersionDto>();

        return await _db.PersonaVersions
            .Where(v => v.PersonaId == personaId)
            .OrderByDescending(v => v.VersionNumber)
            .Select(v => new PersonaVersionDto(
                v.Id,
                v.PersonaId,
                v.VersionNumber,
                v.SystemPrompt,
                v.ConfigurationJson,
                v.IsPublished,
                v.CreatedAt
            ))
            .ToListAsync();
    }

    public async Task<PersonaVersionDto> CreateVersionAsync(Guid personaId, Guid userId, CreatePersonaVersionRequest request)
    {
        var persona = await _db.Personas
            .FirstOrDefaultAsync(p => p.Id == personaId && p.UserId == userId && p.IsActive)
            ?? throw new InvalidOperationException("Persona not found.");

        var maxVersion = await _db.PersonaVersions
            .Where(v => v.PersonaId == personaId)
            .MaxAsync(v => (int?)v.VersionNumber) ?? 0;

        var systemPrompt = request.SystemPrompt;
        var configurationJson = request.ConfigurationJson ?? "{}";

        if (string.IsNullOrWhiteSpace(systemPrompt))
        {
            var latest = await _db.PersonaVersions
                .Where(v => v.PersonaId == personaId)
                .OrderByDescending(v => v.VersionNumber)
                .FirstOrDefaultAsync();

            systemPrompt = latest?.SystemPrompt ?? string.Empty;
            configurationJson = latest?.ConfigurationJson ?? "{}";
        }

        var version = new PersonaVersion
        {
            PersonaId = personaId,
            VersionNumber = maxVersion + 1,
            SystemPrompt = systemPrompt ?? string.Empty,
            ConfigurationJson = configurationJson,
            IsPublished = false,
            CreatedAt = DateTime.UtcNow
        };

        _db.PersonaVersions.Add(version);
        await _db.SaveChangesAsync();

        return new PersonaVersionDto(
            version.Id,
            version.PersonaId,
            version.VersionNumber,
            version.SystemPrompt,
            version.ConfigurationJson,
            version.IsPublished,
            version.CreatedAt
        );
    }

    public async Task<PersonaVersionDto?> GetVersionAsync(Guid personaId, Guid versionId, Guid userId)
    {
        var persona = await _db.Personas
            .FirstOrDefaultAsync(p => p.Id == personaId && p.UserId == userId && p.IsActive);

        if (persona is null)
            return null;

        return await _db.PersonaVersions
            .Where(v => v.Id == versionId && v.PersonaId == personaId)
            .Select(v => new PersonaVersionDto(
                v.Id,
                v.PersonaId,
                v.VersionNumber,
                v.SystemPrompt,
                v.ConfigurationJson,
                v.IsPublished,
                v.CreatedAt
            ))
            .FirstOrDefaultAsync();
    }

    public async Task<PersonaVersionDto?> PublishVersionAsync(Guid personaId, Guid versionId, Guid userId)
    {
        var persona = await _db.Personas
            .FirstOrDefaultAsync(p => p.Id == personaId && p.UserId == userId && p.IsActive);

        if (persona is null)
            return null;

        var version = await _db.PersonaVersions
            .FirstOrDefaultAsync(v => v.Id == versionId && v.PersonaId == personaId);

        if (version is null)
            return null;

        var currentlyPublished = await _db.PersonaVersions
            .Where(v => v.PersonaId == personaId && v.IsPublished)
            .ToListAsync();

        foreach (var published in currentlyPublished)
        {
            published.IsPublished = false;
        }

        version.IsPublished = true;
        await _db.SaveChangesAsync();

        return new PersonaVersionDto(
            version.Id,
            version.PersonaId,
            version.VersionNumber,
            version.SystemPrompt,
            version.ConfigurationJson,
            version.IsPublished,
            version.CreatedAt
        );
    }
}