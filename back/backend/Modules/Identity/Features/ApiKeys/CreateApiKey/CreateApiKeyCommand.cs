using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using backend.Data;
using backend.Dtos;
using backend.Modules.Identity.Models;
using backend.Models.Enums;

namespace backend.Modules.Identity.Features.ApiKeys.CreateApiKey;

public record CreateApiKeyCommand(Guid UserId, CreateApiKeyRequest Request) : IRequest<CreateApiKeyResponse>;

public class CreateApiKeyCommandHandler : IRequestHandler<CreateApiKeyCommand, CreateApiKeyResponse>
{
    private readonly AppDbContext _db;

    public CreateApiKeyCommandHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<CreateApiKeyResponse> Handle(CreateApiKeyCommand command, CancellationToken cancellationToken)
    {
        var rawKey = "sk_live_" + GenerateRandomHex(32);

        var apiKey = new ApiKey
        {
            Id = Guid.NewGuid(),
            UserId = command.UserId,
            Name = command.Request.Name,
            KeyPrefix = rawKey[..15],
            KeyHash = ComputeSha256(rawKey),
            Status = ApiKeyStatus.Active,
            Scopes = command.Request.Scopes ?? Array.Empty<string>(),
            ExpiresAt = command.Request.ExpiresAt,
            CreatedAt = DateTime.UtcNow
        };

        _db.ApiKeys.Add(apiKey);
        await _db.SaveChangesAsync(cancellationToken);

        return new CreateApiKeyResponse(apiKey.Id, apiKey.Name, rawKey, apiKey.KeyPrefix, apiKey.CreatedAt);
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
