using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Dtos;
using backend.Models.Domain;
using backend.Models.Enums;

namespace backend.Services;

public class ActionService
{
    private readonly AppDbContext _db;

    public ActionService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<ActionDefinitionDto>> ListAsync(string? type)
    {
        var query = _db.ActionDefinitions.AsQueryable();

        if (!string.IsNullOrWhiteSpace(type) && Enum.TryParse<ActionType>(type, true, out var actionType))
        {
            query = query.Where(a => a.ActionType == actionType && a.IsActive);
        }
        else
        {
            query = query.Where(a => a.IsActive);
        }

        return await query
            .OrderBy(a => a.DisplayName)
            .Select(a => new ActionDefinitionDto(
                a.Id,
                a.Name,
                a.DisplayName,
                a.Description,
                a.ActionType,
                a.IsSystem,
                a.InputSchemaJson,
                a.OutputSchemaJson,
                a.ConfigurationJson,
                a.IsActive,
                a.CreatedAt,
                a.UpdatedAt
            ))
            .ToListAsync();
    }

    public async Task<ActionDefinitionDto?> GetByIdAsync(Guid id)
    {
        return await _db.ActionDefinitions
            .Where(a => a.Id == id && a.IsActive)
            .Select(a => new ActionDefinitionDto(
                a.Id,
                a.Name,
                a.DisplayName,
                a.Description,
                a.ActionType,
                a.IsSystem,
                a.InputSchemaJson,
                a.OutputSchemaJson,
                a.ConfigurationJson,
                a.IsActive,
                a.CreatedAt,
                a.UpdatedAt
            ))
            .FirstOrDefaultAsync();
    }

    public async Task<ActionDefinitionDto> CreateAsync(CreateActionRequest request)
    {
        if (request.ActionType != ActionType.Integration && request.ActionType != ActionType.Webhook)
            throw new InvalidOperationException("Users can only create Integration or Webhook action definitions.");

        var action = new ActionDefinition
        {
            Name = request.Name,
            DisplayName = request.DisplayName,
            Description = request.Description,
            ActionType = request.ActionType,
            IsSystem = false,
            InputSchemaJson = request.InputSchemaJson,
            OutputSchemaJson = request.OutputSchemaJson,
            ConfigurationJson = request.ConfigurationJson,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.ActionDefinitions.Add(action);
        await _db.SaveChangesAsync();

        return new ActionDefinitionDto(
            action.Id,
            action.Name,
            action.DisplayName,
            action.Description,
            action.ActionType,
            action.IsSystem,
            action.InputSchemaJson,
            action.OutputSchemaJson,
            action.ConfigurationJson,
            action.IsActive,
            action.CreatedAt,
            action.UpdatedAt
        );
    }

    public async Task<ActionDefinitionDto?> UpdateAsync(Guid id, UpdateActionRequest request)
    {
        var action = await _db.ActionDefinitions
            .FirstOrDefaultAsync(a => a.Id == id && a.IsActive);

        if (action is null)
            return null;

        if (action.IsSystem)
            throw new InvalidOperationException("System actions cannot be updated.");

        action.Name = request.Name;
        action.DisplayName = request.DisplayName;
        action.Description = request.Description;
        action.InputSchemaJson = request.InputSchemaJson;
        action.OutputSchemaJson = request.OutputSchemaJson;
        action.ConfigurationJson = request.ConfigurationJson;
        action.IsActive = request.IsActive;
        action.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return new ActionDefinitionDto(
            action.Id,
            action.Name,
            action.DisplayName,
            action.Description,
            action.ActionType,
            action.IsSystem,
            action.InputSchemaJson,
            action.OutputSchemaJson,
            action.ConfigurationJson,
            action.IsActive,
            action.CreatedAt,
            action.UpdatedAt
        );
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var action = await _db.ActionDefinitions
            .FirstOrDefaultAsync(a => a.Id == id && a.IsActive);

        if (action is null)
            return false;

        if (action.IsSystem)
            throw new InvalidOperationException("System actions cannot be deleted.");

        action.IsActive = false;
        action.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return true;
    }

    public async Task<List<ActionDefinitionDto>> ListSystemAsync()
    {
        return await _db.ActionDefinitions
            .Where(a => a.IsSystem && a.IsActive)
            .OrderBy(a => a.DisplayName)
            .Select(a => new ActionDefinitionDto(
                a.Id,
                a.Name,
                a.DisplayName,
                a.Description,
                a.ActionType,
                a.IsSystem,
                a.InputSchemaJson,
                a.OutputSchemaJson,
                a.ConfigurationJson,
                a.IsActive,
                a.CreatedAt,
                a.UpdatedAt
            ))
            .ToListAsync();
    }
}