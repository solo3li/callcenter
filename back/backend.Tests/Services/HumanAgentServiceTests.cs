using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Dtos;
using backend.Models.Domain;
using backend.Models.Enums;
using backend.Services;
using Xunit;

namespace backend.Tests.Services
{
    public class HumanAgentServiceTests
    {
        private AppDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        private async Task<(User, HumanAgentListItem)> SeedUserAndAgent(AppDbContext db)
        {
            var user = new User
            {
                Id = Guid.NewGuid(), Email = "hr@test.com", DisplayName = "HR User",
                PasswordHash = "hash", Status = UserStatus.Active,
                CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
            };
            db.Users.Add(user);
            await db.SaveChangesAsync();

            var service = new HumanAgentService(db);
            var agent = await service.CreateAsync(user.Id, new CreateHumanAgentRequest(
                "Support Agent", "agent@test.com", 3));
            return (user, agent);
        }

        [Fact]
        public async Task CreateAndGet_Works()
        {
            using var db = CreateContext();
            var service = new HumanAgentService(db);
            var user = new User
            {
                Id = Guid.NewGuid(), Email = "create@test.com", DisplayName = "Test",
                PasswordHash = "hash", Status = UserStatus.Active,
                CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
            };
            db.Users.Add(user);
            await db.SaveChangesAsync();

            var agent = await service.CreateAsync(user.Id,
                new CreateHumanAgentRequest("New Agent", "new@test.com", 5));
            Assert.NotNull(agent);
            Assert.Equal("New Agent", agent.Name);
            Assert.Equal(HumanAgentStatus.Offline, agent.Status);

            var list = await service.ListAsync(user.Id);
            Assert.Single(list);
        }

        [Fact]
        public async Task UpdateStatus_Works()
        {
            using var db = CreateContext();
            var service = new HumanAgentService(db);
            var user = new User
            {
                Id = Guid.NewGuid(), Email = "status@test.com", DisplayName = "Test",
                PasswordHash = "hash", Status = UserStatus.Active,
                CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
            };
            db.Users.Add(user);
            await db.SaveChangesAsync();

            var agent = await service.CreateAsync(user.Id,
                new CreateHumanAgentRequest("Agent", null, 2));

            await service.UpdateStatusAsync(agent.Id, user.Id, HumanAgentStatus.Available);
            var updated = await service.GetByIdAsync(agent.Id, user.Id);
            Assert.NotNull(updated);
            Assert.Equal(HumanAgentStatus.Available, updated.Status);
        }

        [Fact]
        public async Task AccessKey_CreateAndValidate()
        {
            using var db = CreateContext();
            var service = new HumanAgentService(db);
            var user = new User
            {
                Id = Guid.NewGuid(), Email = "key@test.com", DisplayName = "Test",
                PasswordHash = "hash", Status = UserStatus.Active,
                CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
            };
            db.Users.Add(user);
            await db.SaveChangesAsync();

            var agent = await service.CreateAsync(user.Id,
                new CreateHumanAgentRequest("KeyAgent", null, 1));

            var accessKeyResponse = await service.CreateAccessKeyAsync(
                agent.Id, user.Id, new CreateAccessKeyRequest("Test Key", null));
            var rawKey = accessKeyResponse.RawKey;
            Assert.NotNull(rawKey);
            Assert.NotEmpty(rawKey);

            var validatedId = await service.ValidateAccessKeyAsync(rawKey);
            Assert.NotNull(validatedId);
            Assert.Equal(agent.Id, validatedId!.Value);
        }

        [Fact]
        public async Task AccessKey_RevokeInvalidates()
        {
            using var db = CreateContext();
            var service = new HumanAgentService(db);
            var user = new User
            {
                Id = Guid.NewGuid(), Email = "revoke@test.com", DisplayName = "Test",
                PasswordHash = "hash", Status = UserStatus.Active,
                CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
            };
            db.Users.Add(user);
            await db.SaveChangesAsync();

            var agent = await service.CreateAsync(user.Id,
                new CreateHumanAgentRequest("RevokeAgent", null, 1));

            var accessKeyResponse = await service.CreateAccessKeyAsync(
                agent.Id, user.Id, new CreateAccessKeyRequest("Temp", null));

            await service.RevokeAccessKeyAsync(agent.Id, accessKeyResponse.Id, user.Id);
            var validatedId = await service.ValidateAccessKeyAsync(accessKeyResponse.RawKey);
            Assert.Null(validatedId);
        }

        [Fact]
        public async Task Delete_SoftDeletes()
        {
            using var db = CreateContext();
            var service = new HumanAgentService(db);
            var user = new User
            {
                Id = Guid.NewGuid(), Email = "delete@test.com", DisplayName = "Test",
                PasswordHash = "hash", Status = UserStatus.Active,
                CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
            };
            db.Users.Add(user);
            await db.SaveChangesAsync();

            var agent = await service.CreateAsync(user.Id,
                new CreateHumanAgentRequest("ToDelete", null, 1));

            await service.DeleteAsync(agent.Id, user.Id);
            var deleted = await service.GetByIdAsync(agent.Id, user.Id);
            Assert.Null(deleted);
        }
    }
}