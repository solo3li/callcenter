using System.Text.Json;
using backend.Data;
using backend.Middleware;
using backend.Models.Domain;
using backend.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace backend.Endpoints;

public static class SipDestinationEndpoints
{
    public sealed record CreateSipDestinationRequest(string Name, string CallTo, string? Description);
    public sealed record UpdateSipDestinationRequest(string? Name, string? CallTo, string? Description, bool? IsEnabled);

    public static WebApplication MapSipDestinationEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/sip/destinations");

        group.MapGet("/", async (AppDbContext db, HttpContext http) =>
        {
            var userId = (Guid)http.Items["UserId"]!;
            var items = await db.SipDestinations
                .Where(d => d.UserId == userId)
                .OrderBy(d => d.Name)
                .Select(d => new
                {
                    d.Id,
                    d.Name,
                    d.Description,
                    d.CallTo,
                    d.IsEnabled,
                    d.CreatedAt,
                    d.UpdatedAt
                })
                .ToListAsync();
            return Results.Ok(items);
        });

        group.MapPost("/", async (CreateSipDestinationRequest req, AppDbContext db, HttpContext http) =>
        {
            var userId = (Guid)http.Items["UserId"]!;
            if (string.IsNullOrWhiteSpace(req.Name) || string.IsNullOrWhiteSpace(req.CallTo))
                return Results.BadRequest(new { error = "Name and CallTo are required" });

            var exists = await db.SipDestinations
                .AnyAsync(d => d.UserId == userId && d.Name.ToLower() == req.Name.Trim().ToLower());
            if (exists)
                return Results.Conflict(new { error = $"Destination '{req.Name}' already exists" });

            var destination = new SipDestination
            {
                UserId = userId,
                Name = req.Name.Trim(),
                Description = req.Description?.Trim(),
                CallTo = req.CallTo.Trim()
            };
            db.SipDestinations.Add(destination);
            await db.SaveChangesAsync();
            return Results.Created($"/api/sip/destinations/{destination.Id}", new
            {
                destination.Id,
                destination.Name,
                destination.Description,
                destination.CallTo,
                destination.IsEnabled
            });
        });

        group.MapPatch("/{id:guid}", async (Guid id, UpdateSipDestinationRequest req,
            AppDbContext db, HttpContext http) =>
        {
            var userId = (Guid)http.Items["UserId"]!;
            var destination = await db.SipDestinations
                .FirstOrDefaultAsync(d => d.Id == id && d.UserId == userId);
            if (destination == null) return Results.NotFound();

            if (req.Name != null)
            {
                var clash = await db.SipDestinations.AnyAsync(d =>
                    d.UserId == userId && d.Id != id &&
                    d.Name.ToLower() == req.Name.Trim().ToLower());
                if (clash) return Results.Conflict(new { error = "Name already in use" });
                destination.Name = req.Name.Trim();
            }
            if (req.CallTo != null) destination.CallTo = req.CallTo.Trim();
            if (req.Description != null) destination.Description = req.Description.Trim();
            if (req.IsEnabled.HasValue) destination.IsEnabled = req.IsEnabled.Value;
            destination.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();

            return Results.Ok(new
            {
                destination.Id,
                destination.Name,
                destination.Description,
                destination.CallTo,
                destination.IsEnabled
            });
        });

        group.MapDelete("/{id:guid}", async (Guid id, AppDbContext db, HttpContext http) =>
        {
            var userId = (Guid)http.Items["UserId"]!;
            var destination = await db.SipDestinations
                .FirstOrDefaultAsync(d => d.Id == id && d.UserId == userId);
            if (destination == null) return Results.NotFound();
            db.SipDestinations.Remove(destination);
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        // ── transfer options for the AI layer (names only, never CallTo) ──
        group.MapGet("/options", async (AppDbContext db, HttpContext http) =>
        {
            var userId = (Guid)http.Items["UserId"]!;
            var agents = await db.HumanAgents
                .Where(a => a.OwnerUserId == userId && a.IsActive)
                .Select(a => new { type = "human", name = a.Name, available = a.Status == HumanAgentStatus.Available })
                .ToListAsync();
            var destinations = await db.SipDestinations
                .Where(d => d.UserId == userId && d.IsEnabled)
                .Select(d => new { type = "destination", name = d.Name, available = true })
                .ToListAsync();
            return Results.Ok(new { agents, destinations });
        });

        return app;
    }
}
