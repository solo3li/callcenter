using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Dtos;
using backend.Models.Domain;
using backend.Models.Enums;

namespace backend.Services
{
    public class PartnerService
    {
        private readonly AppDbContext _db;

        public PartnerService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<List<PartnerDto>> ListAsync()
        {
            var partners = await _db.Partners
                .Where(p => p.IsActive)
                .OrderBy(p => p.OrganizationName)
                .ToListAsync();
            return partners.Select(Map).ToList();
        }

        public async Task<PartnerDto?> GetByIdAsync(Guid id)
        {
            var partner = await _db.Partners.FindAsync(id);
            return partner == null ? null : Map(partner);
        }

        public async Task<PartnerDto?> GetByUserIdAsync(Guid userId)
        {
            var partner = await _db.Partners.FirstOrDefaultAsync(p => p.UserId == userId);
            return partner == null ? null : Map(partner);
        }

        public async Task<PartnerDto?> UpdateAsync(Guid id, UpdatePartnerRequest request)
        {
            var partner = await _db.Partners.FindAsync(id);
            if (partner == null) return null;

            if (request.OrganizationName != null) partner.OrganizationName = request.OrganizationName;
            if (request.ContactEmail != null) partner.ContactEmail = request.ContactEmail;
            if (request.PhoneNumber != null) partner.PhoneNumber = request.PhoneNumber;
            if (request.Website != null) partner.Website = request.Website;
            if (request.Description != null) partner.Description = request.Description;
            if (request.MetadataJson != null) partner.MetadataJson = request.MetadataJson;
            partner.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return Map(partner);
        }

        public async Task<List<PartnerRelationshipDto>> ListCustomersAsync(Guid partnerId)
        {
            var relationships = await _db.PartnerRelationships
                .Where(r => r.PartnerId == partnerId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
            return relationships.Select(MapRelation).ToList();
        }

        public async Task<PartnerRelationshipDto> AddCustomerAsync(
            Guid partnerId, CreateRelationshipRequest request)
        {
            var relationship = new PartnerRelationship
            {
                Id = Guid.NewGuid(),
                PartnerId = partnerId,
                CustomerUserId = request.CustomerUserId,
                Status = PartnerRelationshipStatus.Active,
                MetadataJson = request.MetadataJson,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _db.PartnerRelationships.Add(relationship);
            await _db.SaveChangesAsync();
            return MapRelation(relationship);
        }

        public async Task<PartnerRelationshipDto?> GetRelationshipAsync(Guid relationshipId)
        {
            var rel = await _db.PartnerRelationships.FindAsync(relationshipId);
            return rel == null ? null : MapRelation(rel);
        }

        public async Task<PartnerRelationshipDto?> UpdateRelationshipAsync(
            Guid relationshipId, string? status, string? metadataJson)
        {
            var rel = await _db.PartnerRelationships.FindAsync(relationshipId);
            if (rel == null) return null;

            if (status != null && Enum.TryParse<PartnerRelationshipStatus>(status, true, out var s))
                rel.Status = s;
            if (metadataJson != null)
                rel.MetadataJson = metadataJson;
            rel.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return MapRelation(rel);
        }

        public async Task<bool> DeleteRelationshipAsync(Guid relationshipId)
        {
            var rel = await _db.PartnerRelationships.FindAsync(relationshipId);
            if (rel == null) return false;
            rel.Status = PartnerRelationshipStatus.Inactive;
            rel.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<ProvisionResponse> ProvisionCustomerAsync(
            Guid partnerId, ProvisionRequest request)
        {
            var existing = await _db.PartnerExternalCustomers
                .FirstOrDefaultAsync(ec => ec.PartnerId == partnerId && ec.ExternalCustomerId == request.ExternalCustomerId);

            if (existing != null)
            {
                var existingRel = await _db.PartnerRelationships
                    .FirstOrDefaultAsync(r => r.PartnerId == partnerId && r.CustomerUserId == existing.PlatformUserId);

                var existingLicense = await _db.Licenses
                    .FirstOrDefaultAsync(l => l.UserId == existing.PlatformUserId && l.PartnerId == partnerId);

                var existingKey = await _db.ApiKeys
                    .FirstOrDefaultAsync(k => k.UserId == existing.PlatformUserId && k.Status == ApiKeyStatus.Active);

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
                var plan = await _db.PartnerPlans.FindAsync(request.PartnerPlanId.Value);
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

            await _db.SaveChangesAsync();

            return new ProvisionResponse(
                user.Id,
                relationship.Id,
                license?.Id,
                rawKey);
        }

        public async Task<ProvisionResponse> GetProvisionStatusAsync(
            Guid partnerId, string externalCustomerId)
        {
            var ec = await _db.PartnerExternalCustomers
                .FirstOrDefaultAsync(e => e.PartnerId == partnerId && e.ExternalCustomerId == externalCustomerId);

            if (ec == null)
                throw new InvalidOperationException("Customer not found");

            var rel = await _db.PartnerRelationships
                .FirstOrDefaultAsync(r => r.PartnerId == partnerId && r.CustomerUserId == ec.PlatformUserId);

            var license = await _db.Licenses
                .FirstOrDefaultAsync(l => l.UserId == ec.PlatformUserId && l.PartnerId == partnerId);

            var key = await _db.ApiKeys
                .FirstOrDefaultAsync(k => k.UserId == ec.PlatformUserId && k.Status == ApiKeyStatus.Active);

            return new ProvisionResponse(
                ec.PlatformUserId,
                rel?.Id ?? Guid.Empty,
                license?.Id,
                key?.KeyPrefix ?? string.Empty);
        }

        private static PartnerDto Map(Partner p) => new(
            p.Id, p.UserId, p.OrganizationName, p.ContactEmail,
            p.PhoneNumber, p.Website, p.Description,
            p.IsActive, p.MetadataJson, p.CreatedAt, p.UpdatedAt);

        private static PartnerRelationshipDto MapRelation(PartnerRelationship r) => new(
            r.Id, r.PartnerId, r.CustomerUserId, r.Status.ToString(),
            r.MetadataJson, r.CreatedAt, r.UpdatedAt);
    }
}