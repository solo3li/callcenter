using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediatR;
using backend.Data;
using backend.Models.Enums;

namespace backend.Modules.CallOperations.Features.HumanAgents.ValidateHumanAgentAccessKey;

public record ValidateHumanAgentAccessKeyQuery(string RawKey) : IRequest<Guid?>;

public class ValidateHumanAgentAccessKeyQueryHandler : IRequestHandler<ValidateHumanAgentAccessKeyQuery, Guid?>
{
    private readonly AppDbContext _db;

    public ValidateHumanAgentAccessKeyQueryHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Guid?> Handle(ValidateHumanAgentAccessKeyQuery request, CancellationToken cancellationToken)
    {
        var hash = ComputeSha256(request.RawKey);

        var key = await _db.HumanAgentAccessKeys
            .Include(k => k.HumanAgent)
            .FirstOrDefaultAsync(k => k.KeyHash == hash && k.Status == AccessKeyStatus.Active, cancellationToken);

        if (key is null)
            return null;

        if (key.ExpiresAt.HasValue && key.ExpiresAt.Value < DateTime.UtcNow)
        {
            key.Status = AccessKeyStatus.Expired;
            await _db.SaveChangesAsync(cancellationToken);
            return null;
        }

        if (!key.HumanAgent.IsActive)
            return null;

        key.LastUsedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);

        return key.HumanAgentId;
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
