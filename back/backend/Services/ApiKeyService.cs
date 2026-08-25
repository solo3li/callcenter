using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Dtos;
using backend.Models.Domain;
using backend.Models.Enums;

namespace backend.Services
{
    public class ApiKeyService
    {
        private readonly AppDbContext _db;

        public ApiKeyService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<List<ApiKey>> ListAsync(Guid userId)
        {
            return await _db.ApiKeys
                .Where(k => k.UserId == userId)
                .OrderByDescending(k => k.CreatedAt)
                .ToListAsync();
        }

        public async Task<(ApiKey ApiKey, string RawKey)> CreateAsync(Guid userId, CreateApiKeyRequest request)
        {
            var rawKey = "sk_live_" + GenerateRandomHex(32);

            var apiKey = new ApiKey
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Name = request.Name,
                KeyPrefix = rawKey[..15],
                KeyHash = ComputeSha256(rawKey),
                Status = ApiKeyStatus.Active,
                Scopes = request.Scopes ?? Array.Empty<string>(),
                ExpiresAt = request.ExpiresAt,
                CreatedAt = DateTime.UtcNow
            };

            _db.ApiKeys.Add(apiKey);
            await _db.SaveChangesAsync();
            return (apiKey, rawKey);
        }

        public async Task RevokeAsync(Guid keyId, Guid userId)
        {
            var key = await _db.ApiKeys.FirstOrDefaultAsync(k => k.Id == keyId && k.UserId == userId);
            if (key == null)
                throw new InvalidOperationException("API key not found");

            key.Status = ApiKeyStatus.Revoked;
            key.RevokedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }

        public async Task UpdateScopesAsync(Guid keyId, Guid userId, string[] scopes)
        {
            var key = await _db.ApiKeys.FirstOrDefaultAsync(k => k.Id == keyId && k.UserId == userId);
            if (key == null)
                throw new InvalidOperationException("API key not found");

            key.Scopes = scopes;
            await _db.SaveChangesAsync();
        }

        public async Task<Guid?> ValidateApiKeyAsync(string rawKey)
        {
            var hash = ComputeSha256(rawKey);
            var key = await _db.ApiKeys.FirstOrDefaultAsync(k => k.KeyHash == hash);

            if (key == null)
                return null;

            if (key.Status != ApiKeyStatus.Active)
                return null;

            if (key.ExpiresAt.HasValue && key.ExpiresAt.Value < DateTime.UtcNow)
            {
                key.Status = ApiKeyStatus.Expired;
                await _db.SaveChangesAsync();
                return null;
            }

            key.LastUsedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return key.UserId;
        }

        private static string GenerateRandomHex(int byteCount)
        {
            var bytes = new byte[byteCount];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            return Convert.ToHexString(bytes).ToLowerInvariant();
        }

        private static string ComputeSha256(string input)
        {
            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(hash).ToLowerInvariant();
        }
    }
}