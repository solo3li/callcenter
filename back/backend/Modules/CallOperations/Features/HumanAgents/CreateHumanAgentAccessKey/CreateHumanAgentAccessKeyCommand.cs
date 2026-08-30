using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediatR;
using backend.Data;
using backend.Modules.Identity.Dtos;
using backend.Modules.Billing.Dtos;
using backend.Modules.CallOperations.Dtos;
using backend.Modules.Configuration.Dtos;
using backend.Modules.Analytics.Dtos;
using backend.Models.Domain;
using backend.Models.Enums;

namespace backend.Modules.CallOperations.Features.HumanAgents.CreateHumanAgentAccessKey;

public record CreateHumanAgentAccessKeyCommand(Guid HumanAgentId, Guid OwnerUserId, CreateAccessKeyRequest Request) : IRequest<CreateAccessKeyResponse>;

public class CreateHumanAgentAccessKeyCommandHandler : IRequestHandler<CreateHumanAgentAccessKeyCommand, CreateAccessKeyResponse>
{
    private readonly AppDbContext _db;

    public CreateHumanAgentAccessKeyCommandHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<CreateAccessKeyResponse> Handle(CreateHumanAgentAccessKeyCommand request, CancellationToken cancellationToken)
    {
        var agent = await _db.HumanAgents
            .FirstOrDefaultAsync(a => a.Id == request.HumanAgentId && a.OwnerUserId == request.OwnerUserId && a.IsActive, cancellationToken)
            ?? throw new InvalidOperationException("Human agent not found.");

        var rawKey = GenerateAccessKey();
        var keyHash = ComputeSha256(rawKey);
        var keyPrefix = rawKey[..12];

        var accessKey = new HumanAgentAccessKey
        {
            HumanAgentId = request.HumanAgentId,
            Name = request.Request.Name,
            KeyPrefix = keyPrefix,
            KeyHash = keyHash,
            Status = AccessKeyStatus.Active,
            ExpiresAt = request.Request.ExpiresAt,
            CreatedAt = DateTime.UtcNow
        };

        _db.HumanAgentAccessKeys.Add(accessKey);
        await _db.SaveChangesAsync(cancellationToken);

        return new CreateAccessKeyResponse(
            accessKey.Id,
            accessKey.Name,
            rawKey,
            accessKey.KeyPrefix,
            accessKey.ExpiresAt,
            accessKey.CreatedAt
        );
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
