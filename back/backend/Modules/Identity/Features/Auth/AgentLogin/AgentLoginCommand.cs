using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediatR;
using Jose;
using backend.Data;
using backend.Modules.Identity.Dtos;
using backend.Modules.Billing.Dtos;
using backend.Modules.CallOperations.Dtos;
using backend.Modules.Configuration.Dtos;
using backend.Modules.Analytics.Dtos;
using backend.Models.Enums;

namespace backend.Modules.Identity.Features.Auth.AgentLogin;

public record AgentLoginCommand(string AccessKey) : IRequest<AgentLoginResponse?>;

public class AgentLoginCommandHandler : IRequestHandler<AgentLoginCommand, AgentLoginResponse?>
{
    private readonly AppDbContext _db;

    public AgentLoginCommandHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<AgentLoginResponse?> Handle(AgentLoginCommand request, CancellationToken cancellationToken)
    {
        var rawKeyHash = Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(request.AccessKey))
        ).ToLowerInvariant();

        var accessKey = await _db.HumanAgentAccessKeys
            .Include(k => k.HumanAgent)
            .FirstOrDefaultAsync(k => k.KeyHash == rawKeyHash
                && k.Status == AccessKeyStatus.Active
                && (k.ExpiresAt == null || k.ExpiresAt >= DateTime.UtcNow), cancellationToken);

        if (accessKey == null)
            return null;

        accessKey.LastUsedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);

        var agent = accessKey.HumanAgent;
        var apiKey = Environment.GetEnvironmentVariable("LIVEKIT_API_KEY") ?? "devkey";
        var apiSecret = Environment.GetEnvironmentVariable("LIVEKIT_API_SECRET") ?? "secret";
        var livekitUrl = (Environment.GetEnvironmentVariable("LIVEKIT_URL") ?? "ws://127.0.0.1:7880")
            .Replace("http://", "ws://");

        var identity = $"agent_{agent.Id}";
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        var payload = new Dictionary<string, object>
        {
            { "iss", apiKey },
            { "sub", identity },
            { "name", agent.Name },
            { "nbf", now },
            { "exp", now + 7200 },
            { "video", new Dictionary<string, object>
                {
                    { "roomJoin", true },
                    { "room", "*" },
                    { "canPublish", true },
                    { "canSubscribe", true }
                }
            }
        };

        var livekitToken = JWT.Encode(payload, Encoding.UTF8.GetBytes(apiSecret), JwsAlgorithm.HS256);

        return new AgentLoginResponse(
            agent.Id,
            agent.Name,
            livekitToken,
            livekitUrl,
            agent.OwnerUserId.ToString()
        );
    }
}
