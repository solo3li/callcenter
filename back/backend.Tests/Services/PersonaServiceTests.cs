using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Dtos;
using backend.Services;
using Xunit;

namespace backend.Tests.Services
{
    public class PersonaServiceTests
    {
        private AppDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        [Fact]
        public async Task CreateAndVersionPersona()
        {
            using var db = CreateContext();
            var service = new PersonaService(db);
            var userId = Guid.NewGuid();

            var persona = await service.CreateAsync(userId,
                new CreatePersonaRequest("Sales Bot", "Handles sales inquiries"));
            Assert.NotNull(persona);
            Assert.Equal("Sales Bot", persona.Name);

            var v1 = await service.CreateVersionAsync(persona.Id, userId,
                new CreatePersonaVersionRequest(
                    "You are a helpful sales agent. Be friendly and persuasive.",
                    "{\"model\":\"gemini-3.1\",\"voice\":\"Aoede\"}"));
            Assert.Equal(1, v1.VersionNumber);
            Assert.False(v1.IsPublished);

            var published = await service.PublishVersionAsync(persona.Id, v1.Id, userId);
            Assert.NotNull(published);
            Assert.True(published.IsPublished);

            var v2 = await service.CreateVersionAsync(persona.Id, userId,
                new CreatePersonaVersionRequest(
                    "You are an expert sales closer. Always ask for the sale.",
                    "{\"model\":\"gemini-3.1\",\"voice\":\"Puck\"}"));
            Assert.Equal(2, v2.VersionNumber);

            var versions = await service.ListVersionsAsync(persona.Id, userId);
            var publishedVersion = versions.FirstOrDefault(v => v.IsPublished);
            Assert.NotNull(publishedVersion);
            Assert.Equal(1, publishedVersion!.VersionNumber);

            await service.PublishVersionAsync(persona.Id, v2.Id, userId);
            versions = await service.ListVersionsAsync(persona.Id, userId);
            var newPublished = versions.FirstOrDefault(v => v.IsPublished);
            Assert.NotNull(newPublished);
            Assert.Equal(2, newPublished!.VersionNumber);
        }

        [Fact]
        public async Task ListPersonas_ScopedToUser()
        {
            using var db = CreateContext();
            var service = new PersonaService(db);
            var user1 = Guid.NewGuid();
            var user2 = Guid.NewGuid();

            await service.CreateAsync(user1, new CreatePersonaRequest("P1", null));
            await service.CreateAsync(user1, new CreatePersonaRequest("P2", null));
            await service.CreateAsync(user2, new CreatePersonaRequest("P3", null));

            var user1Personas = await service.ListAsync(user1);
            Assert.Equal(2, user1Personas.Count);

            var user2Personas = await service.ListAsync(user2);
            Assert.Single(user2Personas);
        }
    }
}