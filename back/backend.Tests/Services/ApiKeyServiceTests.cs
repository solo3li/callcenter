using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Dtos;
using backend.Services;
using Xunit;

namespace backend.Tests.Services
{
    public class ApiKeyServiceTests
    {
        private AppDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        [Fact]
        public async Task CreateAndValidate_Works()
        {
            using var db = CreateContext();
            var service = new ApiKeyService(db);
            var userId = Guid.NewGuid();

            var (_, rawKey) = await service.CreateAsync(userId, new CreateApiKeyRequest("Test Key", null, null));
            var validatedUserId = await service.ValidateApiKeyAsync(rawKey);

            Assert.NotNull(validatedUserId);
            Assert.Equal(userId, validatedUserId!.Value);
        }

        [Fact]
        public async Task RevokedKey_FailsValidation()
        {
            using var db = CreateContext();
            var service = new ApiKeyService(db);
            var userId = Guid.NewGuid();

            var (apiKey, rawKey) = await service.CreateAsync(userId, new CreateApiKeyRequest("Test", null, null));
            await service.RevokeAsync(apiKey.Id, userId);
            var result = await service.ValidateApiKeyAsync(rawKey);

            Assert.Null(result);
        }
    }
}