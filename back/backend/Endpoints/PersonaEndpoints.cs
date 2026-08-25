using backend.Data;
using backend.Dtos;
using backend.Models.Domain;
using backend.Services;
using Microsoft.EntityFrameworkCore;

namespace backend.Endpoints;

public static class PersonaEndpoints
{
    public static void MapPersonaEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/personas");

        group.MapGet("/", async (PersonaService service, HttpContext http) =>
        {
            var userId = (Guid)http.Items["UserId"]!;
            var personas = await service.ListAsync(userId);
            return Results.Ok(personas);
        });

        group.MapGet("/{id:guid}", async (Guid id, PersonaService service, HttpContext http) =>
        {
            var userId = (Guid)http.Items["UserId"]!;
            var persona = await service.GetByIdAsync(id, userId);
            return persona is not null ? Results.Ok(persona) : Results.NotFound();
        });

        group.MapPost("/", async (CreatePersonaRequest request, PersonaService service, HttpContext http) =>
        {
            var userId = (Guid)http.Items["UserId"]!;
            var persona = await service.CreateAsync(userId, request);
            return Results.Created($"/api/personas/{persona.Id}", persona);
        });

        group.MapPatch("/{id:guid}", async (Guid id, UpdatePersonaRequest request, PersonaService service, HttpContext http) =>
        {
            var userId = (Guid)http.Items["UserId"]!;
            var persona = await service.UpdateAsync(id, userId, request);
            return persona is not null ? Results.Ok(persona) : Results.NotFound();
        });

        group.MapDelete("/{id:guid}", async (Guid id, PersonaService service, HttpContext http) =>
        {
            var userId = (Guid)http.Items["UserId"]!;
            var deleted = await service.DeleteAsync(id, userId);
            return deleted ? Results.NoContent() : Results.NotFound();
        });

        var actionGroup = group.MapGroup("/{personaId:guid}/actions");

        actionGroup.MapGet("/", async (Guid personaId, AppDbContext db, HttpContext http) =>
        {
            var userId = (Guid)http.Items["UserId"]!;
            var persona = await db.Personas
                .FirstOrDefaultAsync(p => p.Id == personaId && p.UserId == userId && p.IsActive);
            if (persona is null)
                return Results.NotFound();

            var actions = await db.PersonaActions
                .Where(pa => pa.PersonaId == personaId)
                .Include(pa => pa.ActionDefinition)
                .Select(pa => new ActionDefinitionDto(
                    pa.ActionDefinition.Id,
                    pa.ActionDefinition.Name,
                    pa.ActionDefinition.DisplayName,
                    pa.ActionDefinition.Description,
                    pa.ActionDefinition.ActionType,
                    pa.ActionDefinition.IsSystem,
                    pa.ActionDefinition.InputSchemaJson,
                    pa.ActionDefinition.OutputSchemaJson,
                    pa.ActionDefinition.ConfigurationJson,
                    pa.ActionDefinition.IsActive,
                    pa.ActionDefinition.CreatedAt,
                    pa.ActionDefinition.UpdatedAt
                ))
                .ToListAsync();

            return Results.Ok(actions);
        });

        actionGroup.MapPost("/{actionDefinitionId:guid}", async (Guid personaId, Guid actionDefinitionId, AppDbContext db, HttpContext http) =>
        {
            var userId = (Guid)http.Items["UserId"]!;
            var persona = await db.Personas
                .FirstOrDefaultAsync(p => p.Id == personaId && p.UserId == userId && p.IsActive);
            if (persona is null)
                return Results.NotFound();

            var actionDef = await db.ActionDefinitions
                .FirstOrDefaultAsync(a => a.Id == actionDefinitionId && a.IsActive);
            if (actionDef is null)
                return Results.NotFound();

            var exists = await db.PersonaActions
                .AnyAsync(pa => pa.PersonaId == personaId && pa.ActionDefinitionId == actionDefinitionId);
            if (exists)
                return Results.Conflict();

            var personaAction = new PersonaAction
            {
                PersonaId = personaId,
                ActionDefinitionId = actionDefinitionId,
                CreatedAt = DateTime.UtcNow
            };

            db.PersonaActions.Add(personaAction);
            await db.SaveChangesAsync();

            return Results.Created($"/api/personas/{personaId}/actions/{actionDefinitionId}", null);
        });

        actionGroup.MapDelete("/{actionDefinitionId:guid}", async (Guid personaId, Guid actionDefinitionId, AppDbContext db, HttpContext http) =>
        {
            var userId = (Guid)http.Items["UserId"]!;
            var persona = await db.Personas
                .FirstOrDefaultAsync(p => p.Id == personaId && p.UserId == userId && p.IsActive);
            if (persona is null)
                return Results.NotFound();

            var personaAction = await db.PersonaActions
                .FirstOrDefaultAsync(pa => pa.PersonaId == personaId && pa.ActionDefinitionId == actionDefinitionId);
            if (personaAction is null)
                return Results.NotFound();

            db.PersonaActions.Remove(personaAction);
            await db.SaveChangesAsync();

            return Results.NoContent();
        });

        var versionGroup = group.MapGroup("/{personaId:guid}/versions");

        versionGroup.MapGet("/", async (Guid personaId, PersonaService service, HttpContext http) =>
        {
            var userId = (Guid)http.Items["UserId"]!;
            var versions = await service.ListVersionsAsync(personaId, userId);
            return Results.Ok(versions);
        });

        versionGroup.MapPost("/", async (Guid personaId, CreatePersonaVersionRequest request, PersonaService service, HttpContext http) =>
        {
            var userId = (Guid)http.Items["UserId"]!;
            try
            {
                var version = await service.CreateVersionAsync(personaId, userId, request);
                return Results.Created($"/api/personas/{personaId}/versions/{version.Id}", version);
            }
            catch (InvalidOperationException)
            {
                return Results.NotFound();
            }
        });

        versionGroup.MapGet("/{versionId:guid}", async (Guid personaId, Guid versionId, PersonaService service, HttpContext http) =>
        {
            var userId = (Guid)http.Items["UserId"]!;
            var version = await service.GetVersionAsync(personaId, versionId, userId);
            return version is not null ? Results.Ok(version) : Results.NotFound();
        });

        versionGroup.MapPost("/{versionId:guid}/publish", async (Guid personaId, Guid versionId, PersonaService service, HttpContext http) =>
        {
            var userId = (Guid)http.Items["UserId"]!;
            var version = await service.PublishVersionAsync(personaId, versionId, userId);
            return version is not null ? Results.Ok(version) : Results.NotFound();
        });
    }
}