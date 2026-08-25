using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Dtos;
using backend.Models.Domain;

namespace backend.Services
{
    public class CallConfigurationService
    {
        private readonly AppDbContext _db;

        public CallConfigurationService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<List<CallConfigListItem>> ListAsync(Guid userId)
        {
            return await _db.CallConfigurations
                .Where(c => c.UserId == userId)
                .Select(c => new CallConfigListItem(
                    c.Id,
                    c.Name,
                    c.Description,
                    c.PersonaId,
                    c.Persona != null ? c.Persona.Name : null,
                    c.WorkflowId,
                    c.Workflow != null ? c.Workflow.Name : null,
                    c.IsActive,
                    c.ConfigJson,
                    c.CallConfigurationActions.Count,
                    c.CreatedAt,
                    c.UpdatedAt
                ))
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<CallConfigListItem?> GetByIdAsync(Guid id, Guid userId)
        {
            return await _db.CallConfigurations
                .Where(c => c.Id == id && c.UserId == userId)
                .Select(c => new CallConfigListItem(
                    c.Id,
                    c.Name,
                    c.Description,
                    c.PersonaId,
                    c.Persona != null ? c.Persona.Name : null,
                    c.WorkflowId,
                    c.Workflow != null ? c.Workflow.Name : null,
                    c.IsActive,
                    c.ConfigJson,
                    c.CallConfigurationActions.Count,
                    c.CreatedAt,
                    c.UpdatedAt
                ))
                .FirstOrDefaultAsync();
        }

        public async Task<CallConfigListItem> CreateAsync(Guid userId, CreateCallConfigRequest request)
        {
            await ValidateCrossTenantAsync(userId, request.PersonaId, request.WorkflowId);

            var config = new CallConfiguration
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Name = request.Name,
                Description = request.Description,
                PersonaId = request.PersonaId,
                WorkflowId = request.WorkflowId,
                ConfigJson = request.ConfigJson,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _db.CallConfigurations.Add(config);
            await _db.SaveChangesAsync();

            return await GetByIdAsync(config.Id, userId)
                ?? throw new InvalidOperationException("Failed to create call configuration");
        }

        public async Task<CallConfigListItem?> UpdateAsync(Guid id, Guid userId, UpdateCallConfigRequest request)
        {
            var config = await _db.CallConfigurations
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (config == null)
                return null;

            await ValidateCrossTenantAsync(userId, request.PersonaId, request.WorkflowId);

            config.Name = request.Name;
            config.Description = request.Description;
            config.PersonaId = request.PersonaId;
            config.WorkflowId = request.WorkflowId;
            config.ConfigJson = request.ConfigJson;
            config.UpdatedAt = DateTime.UtcNow;

            if (request.IsActive.HasValue)
                config.IsActive = request.IsActive.Value;

            await _db.SaveChangesAsync();

            return await GetByIdAsync(config.Id, userId);
        }

        public async Task<bool> DeleteAsync(Guid id, Guid userId)
        {
            var config = await _db.CallConfigurations
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (config == null)
                return false;

            _db.CallConfigurations.Remove(config);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<CallConfigListItem?> ActivateAsync(Guid id, Guid userId)
        {
            var config = await _db.CallConfigurations
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (config == null)
                return null;

            config.IsActive = true;
            config.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return await GetByIdAsync(config.Id, userId);
        }

        public async Task<CallConfigListItem?> SetActionsAsync(Guid id, Guid userId, SetConfigActionsRequest request)
        {
            var config = await _db.CallConfigurations
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (config == null)
                return null;

            var existingActions = await _db.CallConfigurationActions
                .Where(a => a.CallConfigurationId == id)
                .ToListAsync();

            _db.CallConfigurationActions.RemoveRange(existingActions);

            var newActions = request.ActionDefinitionIds
                .Select(actionId => new CallConfigurationAction
                {
                    CallConfigurationId = id,
                    ActionDefinitionId = actionId,
                    CreatedAt = DateTime.UtcNow
                });

            _db.CallConfigurationActions.AddRange(newActions);
            await _db.SaveChangesAsync();

            return await GetByIdAsync(config.Id, userId);
        }

        public async Task<List<CallConfigurationAction>> GetActionsAsync(Guid id, Guid userId)
        {
            var config = await _db.CallConfigurations
                .AnyAsync(c => c.Id == id && c.UserId == userId);

            if (!config)
                return new List<CallConfigurationAction>();

            return await _db.CallConfigurationActions
                .Where(a => a.CallConfigurationId == id)
                .Include(a => a.ActionDefinition)
                .ToListAsync();
        }

        private async Task ValidateCrossTenantAsync(Guid userId, Guid? personaId, Guid? workflowId)
        {
            if (personaId.HasValue)
            {
                var persona = await _db.Personas.FirstOrDefaultAsync(p => p.Id == personaId.Value);
                if (persona == null)
                    throw new InvalidOperationException("Persona not found");
                if (persona.UserId != userId)
                    throw new InvalidOperationException("Persona does not belong to the current user");
            }

            if (workflowId.HasValue)
            {
                var workflow = await _db.Workflows.FirstOrDefaultAsync(w => w.Id == workflowId.Value);
                if (workflow == null)
                    throw new InvalidOperationException("Workflow not found");
                if (workflow.UserId != userId)
                    throw new InvalidOperationException("Workflow does not belong to the current user");
            }
        }
    }
}