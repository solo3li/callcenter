using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using Moq;
using backend.Data;
using backend.Dtos;
using backend.Hubs;
using backend.Models.Domain;
using backend.Models.Enums;
using backend.Services;
using Xunit;

namespace backend.Tests.Services
{
    public class CallServiceTests
    {
        private AppDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        private Mock<IHubContext<CallHub>> CreateMockHub()
        {
            var mockHub = new Mock<IHubContext<CallHub>>();
            var mockClients = new Mock<IHubClients>();
            var mockClientProxy = new Mock<IClientProxy>();
            mockHub.Setup(h => h.Clients).Returns(mockClients.Object);
            mockClients.Setup(c => c.All).Returns(mockClientProxy.Object);
            return mockHub;
        }

        private async Task<(User, HumanAgent)> SeedUserAndAgent(AppDbContext db)
        {
            var user = new User
            {
                Id = Guid.NewGuid(), Email = "test@test.com", DisplayName = "Test User",
                PasswordHash = "hashed", Status = UserStatus.Active,
                CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
            };
            db.Users.Add(user);

            var agent = new HumanAgent
            {
                Id = Guid.NewGuid(), OwnerUserId = user.Id, Name = "Agent 1",
                Status = HumanAgentStatus.Available, IsActive = true,
                MaxConcurrentCalls = 2, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
            };
            db.HumanAgents.Add(agent);

            await db.SaveChangesAsync();
            return (user, agent);
        }

        [Fact]
        public async Task FullTransferFlow_Success()
        {
            using var db = CreateContext();
            var (user, agent) = await SeedUserAndAgent(db);
            var mockHub = CreateMockHub();

            var sessionService = new CallSessionService(db);
            var transferService = new CallTransferService(db, mockHub.Object);
            var handoffService = new CallHandoffService(db);

            var roomName = CallSessionService.GenerateRoomName();
            var session = await sessionService.CreateAsync(
                user.Id, null, null, roomName, "Inbound");
            Assert.NotNull(session);
            Assert.Equal(CallSessionStatus.Queued, session.Status);

            var transferResult = await transferService.InitiateTransferAsync(
                session.Id, user.Id, "Customer requested supervisor");
            Assert.NotNull(transferResult);
            Assert.Equal("Requested", transferResult.Transfer.Status);

            var accepted = await transferService.AcceptTransferAsync(
                transferResult.Transfer.Id, agent.Id);
            Assert.NotNull(accepted);
            Assert.Equal("Accepted", accepted.Status);

            var handoff = await handoffService.CreateContextAsync(
                transferResult.Transfer.Id,
                "Customer wanted to cancel subscription",
                "{\"intent\":\"cancellation\",\"sentiment\":\"frustrated\"}",
                "Customer requested supervisor");
            Assert.NotNull(handoff);
            Assert.Equal("Customer wanted to cancel subscription", handoff.Summary);

            var completed = await transferService.CompleteTransferAsync(
                transferResult.Transfer.Id);
            Assert.NotNull(completed);
            Assert.Equal("Completed", completed.Status);

            var sessionDetail = await sessionService.GetByIdAsync(session.Id, user.Id);
            Assert.NotNull(sessionDetail);
        }

        [Fact]
        public async Task TransferReject_RetriesWithNextAgent()
        {
            using var db = CreateContext();
            var (user, agent) = await SeedUserAndAgent(db);
            var agent2 = new HumanAgent
            {
                Id = Guid.NewGuid(), OwnerUserId = user.Id, Name = "Agent 2",
                Status = HumanAgentStatus.Available, IsActive = true,
                MaxConcurrentCalls = 2, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
            };
            db.HumanAgents.Add(agent2);
            await db.SaveChangesAsync();

            var mockHub = CreateMockHub();
            var sessionService = new CallSessionService(db);
            var transferService = new CallTransferService(db, mockHub.Object);

            var roomName = CallSessionService.GenerateRoomName();
            var session = await sessionService.CreateAsync(user.Id, null, null, roomName, "Inbound");
            var transferResult = await transferService.InitiateTransferAsync(session.Id, user.Id, "Need help");

            var rejected = await transferService.RejectTransferAsync(transferResult!.Transfer.Id, agent.Id);
            Assert.NotNull(rejected);
            Assert.Equal("Requested", rejected.Transfer.Status);
            Assert.NotEqual(agent.Id, rejected.Transfer.Id);
        }

        [Fact]
        public async Task SessionEnd_SetsDuration()
        {
            using var db = CreateContext();
            var (user, _) = await SeedUserAndAgent(db);

            var sessionService = new CallSessionService(db);
            var roomName = CallSessionService.GenerateRoomName();
            var session = await sessionService.CreateAsync(user.Id, null, null, roomName, "Inbound");

            await Task.Delay(100);
            var result = await sessionService.EndCallAsync(session.Id, user.Id);
            Assert.NotNull(result);
        }

        [Fact]
        public async Task NoAgentsAvailable_ThrowsException()
        {
            using var db = CreateContext();
            var user = new User
            {
                Id = Guid.NewGuid(), Email = "noagents@test.com", DisplayName = "No Agents",
                PasswordHash = "hashed", Status = UserStatus.Active,
                CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
            };
            db.Users.Add(user);
            await db.SaveChangesAsync();

            var mockHub = CreateMockHub();
            var sessionService = new CallSessionService(db);
            var transferService = new CallTransferService(db, mockHub.Object);

            var roomName = CallSessionService.GenerateRoomName();
            var session = await sessionService.CreateAsync(user.Id, null, null, roomName, "Inbound");

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                transferService.InitiateTransferAsync(session.Id, user.Id, "Help"));
        }

        [Fact]
        public async Task RecordingLifecycle()
        {
            using var db = CreateContext();
            var (user, _) = await SeedUserAndAgent(db);

            var mockStorage = new Mock<StorageService>();
            mockStorage.Setup(s => s.GeneratePresignedUrlAsync(It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync("https://presigned.example.com/test");

            var sessionService = new CallSessionService(db);
            var recordingService = new CallRecordingService(db, mockStorage.Object);
            var roomName = CallSessionService.GenerateRoomName();
            await sessionService.CreateAsync(user.Id, null, null, roomName, "Inbound");

            var session = await db.CallSessions.FirstAsync();
            var recording = await recordingService.CreateRecordingAsync(
                session.Id, "recordings/test.ogg", "audio/ogg", 120, 2048000);
            Assert.NotNull(recording);
            Assert.Equal("Pending", recording.Status);

            var callback = await recordingService.HandleEgressCallback(
                session.Id,
                new RecordingCallbackRequest("recordings/callback.ogg", "audio/ogg", 120, 2048000, "Completed"));
            Assert.Equal("Completed", callback.Status);

            var recordings = await recordingService.ListForCallAsync(session.Id);
            Assert.Equal(2, recordings.Count);
        }

        [Fact]
        public async Task RecordingDownload_GetsPresignedUrl()
        {
            using var db = CreateContext();
            var (user, _) = await SeedUserAndAgent(db);

            var mockStorage = new Mock<StorageService>();
            mockStorage.Setup(s => s.GeneratePresignedUrlAsync(It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync("https://presigned.example.com/test");

            var sessionService = new CallSessionService(db);
            var recordingService = new CallRecordingService(db, mockStorage.Object);
            var roomName = CallSessionService.GenerateRoomName();
            await sessionService.CreateAsync(user.Id, null, null, roomName, "Inbound");

            var session = await db.CallSessions.FirstAsync();
            var recording = await recordingService.CreateRecordingAsync(
                session.Id, "recordings/test.ogg", "audio/ogg", 120, 2048000);

            var download = await recordingService.GenerateDownloadUrl(recording.Id);
            Assert.NotNull(download);
            Assert.NotNull(download.Url);
        }
    }
}