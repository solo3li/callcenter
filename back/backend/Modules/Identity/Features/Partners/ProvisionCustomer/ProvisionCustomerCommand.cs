using System;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediatR;
using backend.Data;
using backend.Dtos;
using backend.Modules.Identity.Models;
using backend.Models.Enums;

namespace backend.Modules.Identity.Features.Partners.ProvisionCustomer;

public record ProvisionCustomerCommand(Guid PartnerId, ProvisionRequest Request) : IRequest<ProvisionResponse>;

public class ProvisionCustomerCommandHandler : IRequestHandler<ProvisionCustomerCommand, ProvisionResponse>
{
    private readonly AppDbContext _db;

    public ProvisionCustomerCommandHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<ProvisionResponse> Handle(ProvisionCustomerCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var partnerId = command.PartnerId;

        var existing = await _db.PartnerExternalCustomers
            .FirstOrDefaultAsync(ec => ec.PartnerId == partnerId && ec.ExternalCustomerId == request.ExternalCustomerId, cancellationToken);

        if (existing != null)
        {
            var existingRel = await _db.PartnerRelationships
                .FirstOrDefaultAsync(r => r.PartnerId == partnerId && r.CustomerUserId == existing.PlatformUserId, cancellationToken);

            var existingLicense = await _db.Licenses
                .FirstOrDefaultAsync(l => l.UserId == existing.PlatformUserId && l.PartnerId == partnerId, cancellationToken);

            var existingKey = await _db.ApiKeys
                .FirstOrDefaultAsync(k => k.UserId == existing.PlatformUserId && k.Status == ApiKeyStatus.Active, cancellationToken);

            return new ProvisionResponse(
                existing.PlatformUserId,
                existingRel?.Id ?? Guid.Empty,
                existingLicense?.Id,
                existingKey?.KeyPrefix ?? string.Empty);
        }

        var email = request.Email ?? $"{request.ExternalCustomerId}@partner.local";
        var displayName = request.DisplayName ?? request.ExternalCustomerId;

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            DisplayName = displayName,
            Status = UserStatus.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.Users.Add(user);

        var relationship = new PartnerRelationship
        {
            Id = Guid.NewGuid(),
            PartnerId = partnerId,
            CustomerUserId = user.Id,
            Status = PartnerRelationshipStatus.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.PartnerRelationships.Add(relationship);

        var externalCustomer = new PartnerExternalCustomer
        {
            Id = Guid.NewGuid(),
            PartnerId = partnerId,
            ExternalCustomerId = request.ExternalCustomerId,
            PlatformUserId = user.Id,
            CreatedAt = DateTime.UtcNow
        };

        _db.PartnerExternalCustomers.Add(externalCustomer);

        License? license = null;
        if (request.PartnerPlanId.HasValue)
        {
            var plan = await _db.PartnerPlans.FindAsync(new object[] { request.PartnerPlanId.Value }, cancellationToken);
            if (plan != null)
            {
                license = new License
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    PartnerId = partnerId,
                    PartnerPlanId = request.PartnerPlanId,
                    Status = LicenseStatus.Active,
                    StartsAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _db.Licenses.Add(license);
            }
        }

        var rawKey = Guid.NewGuid().ToString("N") + Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        var keyPrefix = rawKey[..8];
        var keyHash = Convert.ToHexString(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(rawKey))).ToLowerInvariant();

        var apiKey = new ApiKey
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Name = $"partner-provision-{keyPrefix}",
            KeyPrefix = keyPrefix,
            KeyHash = keyHash,
            Status = ApiKeyStatus.Active,
            Scopes = new[] { "api" },
            CreatedAt = DateTime.UtcNow
        };

        _db.ApiKeys.Add(apiKey);

        await _db.SaveChangesAsync(cancellationToken);

        return new ProvisionResponse(
            user.Id,
            relationship.Id,
            license?.Id,
            rawKey);
    }
}
