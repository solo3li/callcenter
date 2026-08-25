using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Dtos;
using backend.Models.Domain;

namespace backend.Services
{
    public class WorkflowService
    {
        private readonly AppDbContext _db;

        public WorkflowService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<List<WorkflowListItem>> ListAsync(Guid userId)
        {
            var workflows = await _db.Workflows
                .Where(w => w.UserId == userId)
                .Include(w => w.Versions)
                .OrderByDescending(w => w.CreatedAt)
                .ToListAsync();

            return workflows.Select(w => new WorkflowListItem(
                w.Id, w.Name, w.Description, w.IsActive,
                w.Versions.Count, w.CreatedAt, w.UpdatedAt)).ToList();
        }

        public async Task<Workflow?> GetByIdAsync(Guid id)
        {
            return await _db.Workflows
                .Include(w => w.Versions)
                .FirstOrDefaultAsync(w => w.Id == id);
        }

        public async Task<Workflow> CreateAsync(Guid userId, CreateWorkflowRequest request)
        {
            var workflow = new Workflow
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Name = request.Name,
                Description = request.Description,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _db.Workflows.Add(workflow);
            await _db.SaveChangesAsync();
            return workflow;
        }

        public async Task<Workflow?> UpdateAsync(Guid id, UpdateWorkflowRequest request)
        {
            var workflow = await _db.Workflows.FindAsync(id);
            if (workflow == null) return null;

            if (request.Name != null) workflow.Name = request.Name;
            if (request.Description != null) workflow.Description = request.Description;
            if (request.IsActive.HasValue) workflow.IsActive = request.IsActive.Value;
            workflow.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return workflow;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var workflow = await _db.Workflows.FindAsync(id);
            if (workflow == null) return false;
            _db.Workflows.Remove(workflow);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<List<WorkflowVersionDto>> ListVersionsAsync(Guid workflowId)
        {
            var versions = await _db.WorkflowVersions
                .Where(v => v.WorkflowId == workflowId)
                .OrderByDescending(v => v.VersionNumber)
                .ToListAsync();

            return versions.Select(v => new WorkflowVersionDto(
                v.Id, v.WorkflowId, v.VersionNumber,
                v.DefinitionJson, v.IsPublished, v.CreatedAt)).ToList();
        }

        public async Task<WorkflowVersionDto> CreateVersionAsync(
            Guid workflowId, CreateWorkflowVersionRequest request)
        {
            var maxVersion = await _db.WorkflowVersions
                .Where(v => v.WorkflowId == workflowId)
                .MaxAsync(v => (int?)v.VersionNumber) ?? 0;

            var version = new WorkflowVersion
            {
                Id = Guid.NewGuid(),
                WorkflowId = workflowId,
                VersionNumber = maxVersion + 1,
                DefinitionJson = request.DefinitionJson,
                IsPublished = true,
                CreatedAt = DateTime.UtcNow
            };

            _db.WorkflowVersions.Add(version);
            await _db.SaveChangesAsync();
            return new WorkflowVersionDto(
                version.Id, version.WorkflowId, version.VersionNumber,
                version.DefinitionJson, version.IsPublished, version.CreatedAt);
        }

        public async Task<WorkflowVersionDto?> GetVersionAsync(Guid versionId)
        {
            var v = await _db.WorkflowVersions.FindAsync(versionId);
            if (v == null) return null;
            return new WorkflowVersionDto(
                v.Id, v.WorkflowId, v.VersionNumber,
                v.DefinitionJson, v.IsPublished, v.CreatedAt);
        }

        public async Task<WorkflowVersionDto?> PublishVersionAsync(Guid versionId)
        {
            var version = await _db.WorkflowVersions.FindAsync(versionId);
            if (version == null) return null;

            var workflow = await _db.Workflows
                .Include(w => w.Versions)
                .FirstOrDefaultAsync(w => w.Id == version.WorkflowId);

            if (workflow != null)
            {
                foreach (var v in workflow.Versions)
                    v.IsPublished = false;
            }

            version.IsPublished = true;
            await _db.SaveChangesAsync();

            return new WorkflowVersionDto(
                version.Id, version.WorkflowId, version.VersionNumber,
                version.DefinitionJson, version.IsPublished, version.CreatedAt);
        }
    }
}