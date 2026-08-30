using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediatR;
using backend.Data;
using backend.Dtos;
using backend.Models.Enums;

namespace backend.Modules.CallOperations.Features.CallSessions.ListCallSessions;

public record ListCallSessionsQuery(
    Guid UserId,
    string? Status,
    string? Direction,
    DateTime? From,
    DateTime? To,
    int Page = 1,
    int Limit = 20) : IRequest<(List<CallSessionListItem> Items, int TotalCount)>;

public class ListCallSessionsQueryHandler : IRequestHandler<ListCallSessionsQuery, (List<CallSessionListItem> Items, int TotalCount)>
{
    private readonly AppDbContext _db;

    public ListCallSessionsQueryHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<(List<CallSessionListItem> Items, int TotalCount)> Handle(ListCallSessionsQuery request, CancellationToken cancellationToken)
    {
        var query = _db.CallSessions
            .Where(c => c.UserId == request.UserId)
            .AsQueryable();

        if (!string.IsNullOrEmpty(request.Status) && Enum.TryParse<CallSessionStatus>(request.Status, true, out var statusEnum))
            query = query.Where(c => c.Status == statusEnum);

        if (!string.IsNullOrEmpty(request.Direction) && Enum.TryParse<CallDirection>(request.Direction, true, out var dirEnum))
            query = query.Where(c => c.Direction == dirEnum);

        if (request.From.HasValue)
            query = query.Where(c => c.StartedAt >= request.From.Value);

        if (request.To.HasValue)
            query = query.Where(c => c.StartedAt <= request.To.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(c => c.StartedAt)
            .Skip((request.Page - 1) * request.Limit)
            .Take(request.Limit)
            .Select(c => new CallSessionListItem(
                c.Id,
                c.UserId,
                c.CallConfigurationId,
                c.LivekitRoomName,
                c.Status.ToString(),
                c.Direction.ToString(),
                c.StartedAt,
                c.AnsweredAt,
                c.EndedAt,
                c.DurationSeconds,
                c.MetadataJson,
                c.Participants.Count,
                c.CreatedAt
            ))
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}
