using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Dtos;
using backend.Services;
using Xunit;

namespace backend.Tests.Services
{
    public class AuthServiceTests
    {
        private AppDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        [Fact]
        public async Task RegisterAsync_CreatesUser()
        {
            using var db = CreateContext();
            var service = new AuthService(db);
            var request = new RegisterRequest("test@test.com", "password123", "Test User", null);
            var user = await service.RegisterAsync(request);
            Assert.NotNull(user);
            Assert.Equal("test@test.com", user.Email);
            Assert.NotEmpty(user.PasswordHash);
        }

        [Fact]
        public async Task RegisterAsync_DuplicateEmail_Throws()
        {
            using var db = CreateContext();
            var service = new AuthService(db);
            await service.RegisterAsync(new RegisterRequest("dup@test.com", "password123", "User", null));
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.RegisterAsync(new RegisterRequest("dup@test.com", "password123", "User2", null)));
        }

        [Fact]
        public async Task LoginAsync_ValidCredentials_ReturnsToken()
        {
            using var db = CreateContext();
            var service = new AuthService(db);
            await service.RegisterAsync(new RegisterRequest("login@test.com", "password123", "User", null));
            var token = await service.LoginAsync(new LoginRequest("login@test.com", "password123"));
            Assert.NotNull(token);
            Assert.NotEmpty(token);
        }

        [Fact]
        public async Task LoginAsync_InvalidPassword_Throws()
        {
            using var db = CreateContext();
            var service = new AuthService(db);
            await service.RegisterAsync(new RegisterRequest("bad@test.com", "password123", "User", null));
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                service.LoginAsync(new LoginRequest("bad@test.com", "wrong")));
        }

        [Fact]
        public async Task ValidateToken_ReturnsUserId()
        {
            using var db = CreateContext();
            var service = new AuthService(db);
            var user = await service.RegisterAsync(new RegisterRequest("token@test.com", "password123", "User", null));
            var token = service.GenerateJwt(user);

            var userId = await service.ValidateTokenAsync(token);
            Assert.NotNull(userId);
            Assert.Equal(user.Id, userId!.Value);
        }

        [Fact]
        public async Task ValidateToken_InvalidToken_ReturnsNull()
        {
            using var db = CreateContext();
            var service = new AuthService(db);
            var userId = await service.ValidateTokenAsync("invalid.token.here");
            Assert.Null(userId);
        }
    }
}