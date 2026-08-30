using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using MediatR;
using System;
using System.Linq;
using backend.Data;
using backend.Modules.CallOperations.Dtos;
using backend.Modules.Analytics.Dtos;
using backend.Services;
using backend.Models.Enums;
using backend.Models.Domain;

namespace backend.Infrastructure.Endpoints
{
    public static class LegacyShimsEndpoints
    {
        public static void MapLegacyShimsEndpoints(this WebApplication app)
        {
            app.MapPost("/api/call/transfer", async (
                TransferShimDto req, HttpContext http, IMediator mediator,
                CallTransferService transferService, AppDbContext db) =>
            {
                if (!backend.Middleware.ServiceAuth.IsConfiguredOrValid(http))
                    return Results.Unauthorized();

                var session = await db.CallSessions
                    .FirstOrDefaultAsync(c => c.LivekitRoomName == req.RoomName);
                if (session == null)
                    return Results.BadRequest(new { error = "Call session not found for room" });
                try
                {
                    var targetType = req.TargetType?.Trim().ToLowerInvariant();

                    if (targetType == "destination")
                    {
                        if (string.IsNullOrWhiteSpace(req.TargetName))
                            return Results.BadRequest(new { error = "TargetName is required for destination transfers" });

                        var destResult = await transferService.InitiateDestinationTransferAsync(
                            session.Id, session.UserId, req.TargetName!, req.Reason);
                        return destResult == null
                            ? Results.NotFound(new { error = "Call session not found" })
                            : Results.Ok(new { transferId = destResult.Id, status = destResult.Status });
                    }

                    // Human transfer (named agent when provided, otherwise best available).
                    Guid? preferredId = Guid.TryParse(req.AgentId, out var aid) ? aid : null;
                    var result = await transferService.InitiateTransferAsync(
                        session.Id, session.UserId, req.Reason,
                        preferredAgentId: preferredId,
                        preferredAgentName: req.TargetName);

                    return Results.Ok(new
                    {
                        transferId = result!.Transfer.Id,
                        agentName = result.Transfer.ToHumanAgentName,
                        status = result.Transfer.Status
                    });
                }
                catch (InvalidOperationException ex)
                {
                    return Results.BadRequest(new { error = ex.Message });
                }
            });

            app.MapPost("/api/call/active", async (
                TransferShimDto req, HttpContext http, IMediator mediator,
                LiveKitService liveKit, AppDbContext db) =>
            {
                if (!backend.Middleware.ServiceAuth.IsConfiguredOrValid(http))
                    return Results.Unauthorized();

                var existing = await db.CallSessions
                    .FirstOrDefaultAsync(c => c.LivekitRoomName == req.RoomName);
                if (existing == null && !req.RoomName.StartsWith(InboundRoutingService.RoomOwnerPrefix))
                {
                    // Platform-managed SIP rooms are created by the webhook path; only
                    // legacy ad-hoc rooms are registered here.
                    await mediator.Send(new backend.Modules.CallOperations.Features.CallSessions.CreateCallSession.CreateCallSessionCommand(
                        Guid.NewGuid(), null, null, req.RoomName, "Inbound"));
                }
                return Results.Ok();
            });

            app.MapPost("/api/call/end", async (
                TransferShimDto req, HttpContext http, IMediator mediator, AppDbContext db) =>
            {
                if (!backend.Middleware.ServiceAuth.IsConfiguredOrValid(http))
                    return Results.Unauthorized();

                var session = await db.CallSessions
                    .FirstOrDefaultAsync(c => c.LivekitRoomName == req.RoomName);
                if (session != null)
                {
                    await mediator.Send(new backend.Modules.CallOperations.Features.CallSessions.EndCallSession.EndCallSessionCommand(session.Id, session.UserId));
                }
                return Results.Ok();
            });

            app.MapPost("/api/call/summary", async (
                SummaryShimDto req, HttpContext http, CallHandoffService handoffService, AppDbContext db) =>
            {
                if (!backend.Middleware.ServiceAuth.IsConfiguredOrValid(http))
                    return Results.Unauthorized();

                var session = await db.CallSessions
                    .FirstOrDefaultAsync(c => c.LivekitRoomName == req.RoomName);
                if (session == null) return Results.Ok();
                var handoff = await db.CallHandoffs
                    .FirstOrDefaultAsync(h => h.CallSessionId == session.Id);
                if (handoff != null)
                {
                    await handoffService.CreateContextAsync(
                        handoff.CallTransferId, req.Summary, null, null);
                }
                return Results.Ok();
            });

            // ── Agent-App Transfer Shims (exempt namespace, optional service token) ──
            app.MapGet("/api/call/transfer-options", async (
                string roomName, Guid agentId, HttpContext http, AppDbContext db) =>
            {
                if (!backend.Middleware.ServiceAuth.IsConfiguredOrValid(http))
                    return Results.Unauthorized();

                var session = await db.CallSessions
                    .FirstOrDefaultAsync(c => c.LivekitRoomName == roomName);
                if (session == null ||
                    (session.Status != CallSessionStatus.Transferred
                        && session.Status != CallSessionStatus.Active))
                    return Results.NotFound(new { error = "No active transferred call" });

                var requester = await db.HumanAgents
                    .FirstOrDefaultAsync(a => a.Id == agentId && a.IsActive && a.OwnerUserId == session.UserId);
                if (requester == null)
                    return Results.Forbid();

                var agents = await db.HumanAgents
                    .Where(a => a.OwnerUserId == session.UserId && a.IsActive && a.Id != agentId)
                    .OrderBy(a => a.Name)
                    .Select(a => new { id = a.Id, name = a.Name, available = a.Status == HumanAgentStatus.Available })
                    .ToListAsync();

                var destinations = await db.SipDestinations
                    .Where(d => d.UserId == session.UserId && d.IsEnabled)
                    .OrderBy(d => d.Name)
                    .Select(d => new { id = d.Id, name = d.Name })
                    .ToListAsync();

                return Results.Ok(new { agents, destinations });
            });

            app.MapPost("/api/call/agent-transfer", async (
                AgentTransferShimDto req, HttpContext http, AppDbContext db, CallTransferService transferService) =>
            {
                if (!backend.Middleware.ServiceAuth.IsConfiguredOrValid(http))
                    return Results.Unauthorized();

                var session = await db.CallSessions
                    .FirstOrDefaultAsync(c => c.LivekitRoomName == req.RoomName);
                if (session == null)
                    return Results.NotFound(new { error = "Call session not found" });

                try
                {
                    var targetType = req.TargetType?.Trim().ToLowerInvariant();
                    CallTransferDto? result;

                    if (targetType == "destination")
                    {
                        if (string.IsNullOrWhiteSpace(req.TargetName))
                            return Results.BadRequest(new { error = "TargetName is required for destination transfers" });
                        result = await transferService.InitiateAgentDestinationTransferAsync(
                            session.Id, req.FromAgentId, req.TargetName!, req.Reason);
                    }
                    else
                    {
                        result = await transferService.InitiateAgentHumanTransferAsync(
                            session.Id, req.FromAgentId, req.TargetName, req.Reason);
                    }

                    return result == null
                        ? Results.BadRequest(new { error = "Requesting agent is not on this call" })
                        : Results.Ok(new { transferId = result.Id, targetName = result.ToHumanAgentName, status = result.Status });
                }
                catch (InvalidOperationException ex)
                {
                    return Results.BadRequest(new { error = ex.Message });
                }
            });

            app.MapPost("/api/call/transfer-decision", async (
                TransferDecisionShimDto req, HttpContext http, CallTransferService transferService) =>
            {
                if (!backend.Middleware.ServiceAuth.IsConfiguredOrValid(http))
                    return Results.Unauthorized();

                try
                {
                    object? result = req.Decision == "accept"
                        ? await transferService.AcceptTransferAsync(req.TransferId, req.HumanAgentId)
                        : await transferService.RejectTransferAsync(req.TransferId, req.HumanAgentId);

                    return result == null
                        ? Results.NotFound(new { error = "Transfer not found for this agent" })
                        : Results.Ok(new { status = "ok" });
                }
                catch (InvalidOperationException ex)
                {
                    return Results.BadRequest(new { error = ex.Message });
                }
            });

            // ── Legacy backward-compat endpoints ──
            app.MapGet("/api/token", async (
                string? identity, string? room,
                LiveKitService liveKit, AppDbContext db) =>
            {
                identity ??= "web-user-" + Guid.NewGuid().ToString("N")[..8];
                var roomName = room ?? backend.Modules.CallOperations.Features.CallSessions.CreateCallSession.CallSessionHelpers.GenerateRoomName();

                var token = liveKit.GenerateToken(identity, roomName, true, true);

                if (!identity.StartsWith("admin_") && !identity.StartsWith("agent_"))
                {
                    var existing = await db.Calls.FirstOrDefaultAsync(c => c.RoomName == roomName);
                    if (existing == null)
                    {
                        db.Calls.Add(new backend.Models.CallRecord
                        {
                            RoomName = roomName, CallerId = identity,
                            Status = "Active", RecordingUrl = $"/recordings/{roomName}.ogg"
                        });
                        await db.SaveChangesAsync();
                    }
                }
                return Results.Json(new { token, url = "ws://127.0.0.1:7880", roomName });
            });
        }
    }

    public class TransferShimDto
    {
        public required string RoomName { get; set; }
        public string? AgentId { get; set; }
        public string? TargetType { get; set; }
        public string? TargetName { get; set; }
        public string? Reason { get; set; }
    }
    public class AgentTransferShimDto
    {
        public required string RoomName { get; set; }
        public required Guid FromAgentId { get; set; }
        public string? TargetType { get; set; }
        public string? TargetName { get; set; }
        public string? Reason { get; set; }
    }
    public class TransferDecisionShimDto
    {
        public required Guid TransferId { get; set; }
        public required Guid HumanAgentId { get; set; }
        public string Decision { get; set; } = "accept";
    }
    public class SummaryShimDto { public required string RoomName { get; set; } public required string Summary { get; set; } }
}
