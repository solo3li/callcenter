using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models.Enums;

namespace backend.Modules.Identity.Services;

public class ApiKeyValidationService
{
    private readonly AppDbContext _db;

    public ApiKeyValidationService(AppDbContext db)
    {
        _db = db;
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

    private static string ComputeSha256(string input)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
