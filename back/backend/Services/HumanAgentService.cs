using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Dtos;
using backend.Models.Domain;
using backend.Models.Enums;

namespace backend.Services;

public class HumanAgentService
{
    private readonly AppDbContext _db;

    public HumanAgentService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<HumanAgentListItem>> ListAsync(Guid ownerUserId)
    {
        return await _db.HumanAgents
            .Where(a => a.OwnerUserId == ownerUserId && a.IsActive)
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new HumanAgentListItem(
                a.Id,
                a.Name,
                a.Email,
                a.Status,
                a.IsActive,
                a.MaxConcurrentCalls,
                a.CreatedAt,
                a.UpdatedAt
            ))
            .ToListAsync();
    }

    public async Task<HumanAgentListItem?> GetByIdAsync(Guid id, Guid ownerUserId)
    {
        return await _db.HumanAgents
            .Where(a => a.Id == id && a.OwnerUserId == ownerUserId && a.IsActive)
            .Select(a => new HumanAgentListItem(
                a.Id,
                a.Name,
                a.Email,
                a.Status,
                a.IsActive,
                a.MaxConcurrentCalls,
                a.CreatedAt,
                a.UpdatedAt
            ))
            .FirstOrDefaultAsync();
    }

    public async Task<HumanAgentListItem> CreateAsync(Guid ownerUserId, CreateHumanAgentRequest request)
    {
        var agent = new HumanAgent
        {
            OwnerUserId = ownerUserId,
            Name = request.Name,
            Email = request.Email,
            MaxConcurrentCalls = request.MaxConcurrentCalls,
            Status = HumanAgentStatus.Offline,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.HumanAgents.Add(agent);
        await _db.SaveChangesAsync();

        return new HumanAgentListItem(
            agent.Id,
            agent.Name,
            agent.Email,
            agent.Status,
            agent.IsActive,
            agent.MaxConcurrentCalls,
            agent.CreatedAt,
            agent.UpdatedAt
        );
    }

    public async Task<HumanAgentListItem?> UpdateAsync(Guid id, Guid ownerUserId, UpdateHumanAgentRequest request)
    {
        var agent = await _db.HumanAgents
            .FirstOrDefaultAsync(a => a.Id == id && a.OwnerUserId == ownerUserId && a.IsActive);

        if (agent is null)
            return null;

        agent.Name = request.Name;
        agent.Email = request.Email;
        agent.MaxConcurrentCalls = request.MaxConcurrentCalls;
        agent.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return new HumanAgentListItem(
            agent.Id,
            agent.Name,
            agent.Email,
            agent.Status,
            agent.IsActive,
            agent.MaxConcurrentCalls,
            agent.CreatedAt,
            agent.UpdatedAt
        );
    }

    public async Task<bool> DeleteAsync(Guid id, Guid ownerUserId)
    {
        var agent = await _db.HumanAgents
            .FirstOrDefaultAsync(a => a.Id == id && a.OwnerUserId == ownerUserId && a.IsActive);

        if (agent is null)
            return false;

        agent.IsActive = false;
        agent.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return true;
    }

    public async Task<HumanAgentListItem?> UpdateStatusAsync(Guid id, Guid ownerUserId, HumanAgentStatus status)
    {
        var agent = await _db.HumanAgents
            .FirstOrDefaultAsync(a => a.Id == id && a.OwnerUserId == ownerUserId && a.IsActive);

        if (agent is null)
            return null;

        agent.Status = status;
        agent.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return new HumanAgentListItem(
            agent.Id,
            agent.Name,
            agent.Email,
            agent.Status,
            agent.IsActive,
            agent.MaxConcurrentCalls,
            agent.CreatedAt,
            agent.UpdatedAt
        );
    }

    public async Task ResetAllToOfflineAsync()
    {
        var agents = await _db.HumanAgents
            .Where(a => a.Status != HumanAgentStatus.Offline)
            .ToListAsync();

        foreach (var agent in agents)
        {
            agent.Status = HumanAgentStatus.Offline;
            agent.UpdatedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
    }

    public async Task<CreateAccessKeyResponse> CreateAccessKeyAsync(Guid humanAgentId, Guid ownerUserId, CreateAccessKeyRequest request)
    {
        var agent = await _db.HumanAgents
            .FirstOrDefaultAsync(a => a.Id == humanAgentId && a.OwnerUserId == ownerUserId && a.IsActive)
            ?? throw new InvalidOperationException("Human agent not found.");

        var rawKey = GenerateAccessKey();
        var keyHash = ComputeSha256(rawKey);
        var keyPrefix = rawKey[..12];

        var accessKey = new HumanAgentAccessKey
        {
            HumanAgentId = humanAgentId,
            Name = request.Name,
            KeyPrefix = keyPrefix,
            KeyHash = keyHash,
            Status = AccessKeyStatus.Active,
            ExpiresAt = request.ExpiresAt,
            CreatedAt = DateTime.UtcNow
        };

        _db.HumanAgentAccessKeys.Add(accessKey);
        await _db.SaveChangesAsync();

        return new CreateAccessKeyResponse(
            accessKey.Id,
            accessKey.Name,
            rawKey,
            accessKey.KeyPrefix,
            accessKey.ExpiresAt,
            accessKey.CreatedAt
        );
    }

    public async Task<List<AccessKeyListItem>> ListAccessKeysAsync(Guid humanAgentId, Guid ownerUserId)
    {
        var agent = await _db.HumanAgents
            .FirstOrDefaultAsync(a => a.Id == humanAgentId && a.OwnerUserId == ownerUserId && a.IsActive);

        if (agent is null)
            return new List<AccessKeyListItem>();

        return await _db.HumanAgentAccessKeys
            .Where(k => k.HumanAgentId == humanAgentId)
            .OrderByDescending(k => k.CreatedAt)
            .Select(k => new AccessKeyListItem(
                k.Id,
                k.Name,
                k.KeyPrefix,
                k.Status,
                k.ExpiresAt,
                k.LastUsedAt,
                k.RevokedAt,
                k.CreatedAt
            ))
            .ToListAsync();
    }

    public async Task<bool> RevokeAccessKeyAsync(Guid humanAgentId, Guid keyId, Guid ownerUserId)
    {
        var agent = await _db.HumanAgents
            .FirstOrDefaultAsync(a => a.Id == humanAgentId && a.OwnerUserId == ownerUserId && a.IsActive);

        if (agent is null)
            return false;

        var key = await _db.HumanAgentAccessKeys
            .FirstOrDefaultAsync(k => k.Id == keyId && k.HumanAgentId == humanAgentId && k.Status == AccessKeyStatus.Active);

        if (key is null)
            return false;

        key.Status = AccessKeyStatus.Revoked;
        key.RevokedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return true;
    }

    public async Task<Guid?> ValidateAccessKeyAsync(string rawKey)
    {
        var hash = ComputeSha256(rawKey);

        var key = await _db.HumanAgentAccessKeys
            .Include(k => k.HumanAgent)
            .FirstOrDefaultAsync(k => k.KeyHash == hash && k.Status == AccessKeyStatus.Active);

        if (key is null)
            return null;

        if (key.ExpiresAt.HasValue && key.ExpiresAt.Value < DateTime.UtcNow)
        {
            key.Status = AccessKeyStatus.Expired;
            await _db.SaveChangesAsync();
            return null;
        }

        if (!key.HumanAgent.IsActive)
            return null;

        key.LastUsedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return key.HumanAgentId;
    }

    public async Task<List<AgentSessionDto>> ListSessionsAsync(Guid humanAgentId, Guid ownerUserId)
    {
        var agent = await _db.HumanAgents
            .FirstOrDefaultAsync(a => a.Id == humanAgentId && a.OwnerUserId == ownerUserId && a.IsActive);

        if (agent is null)
            return new List<AgentSessionDto>();

        return await _db.HumanAgentSessions
            .Where(s => s.HumanAgentId == humanAgentId)
            .OrderByDescending(s => s.ConnectedAt)
            .Select(s => new AgentSessionDto(
                s.Id,
                s.HumanAgentId,
                s.LivekitIdentity,
                s.Status,
                s.ConnectedAt,
                s.DisconnectedAt,
                s.LastHeartbeatAt,
                s.MetadataJson
            ))
            .ToListAsync();
    }

    public async Task<AgentSessionDto?> GetCurrentSessionAsync(Guid humanAgentId, Guid ownerUserId)
    {
        var agent = await _db.HumanAgents
            .FirstOrDefaultAsync(a => a.Id == humanAgentId && a.OwnerUserId == ownerUserId && a.IsActive);

        if (agent is null)
            return null;

        return await _db.HumanAgentSessions
            .Where(s => s.HumanAgentId == humanAgentId && s.Status == "active")
            .OrderByDescending(s => s.ConnectedAt)
            .Select(s => new AgentSessionDto(
                s.Id,
                s.HumanAgentId,
                s.LivekitIdentity,
                s.Status,
                s.ConnectedAt,
                s.DisconnectedAt,
                s.LastHeartbeatAt,
                s.MetadataJson
            ))
            .FirstOrDefaultAsync();
    }

    private static string GenerateAccessKey()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes);
    }

    private static string ComputeSha256(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        var sb = new StringBuilder(bytes.Length * 2);
        foreach (var b in bytes)
            sb.Append(b.ToString("x2"));
        return sb.ToString();
    }
}