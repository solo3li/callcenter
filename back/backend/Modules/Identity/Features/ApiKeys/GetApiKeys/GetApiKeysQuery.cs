using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediatR;
using backend.Data;
using backend.Dtos;

namespace backend.Modules.Identity.Features.ApiKeys.GetApiKeys;

public record GetApiKeysQuery(Guid UserId) : IRequest<List<ApiKeyListItem>>;

public class GetApiKeysQueryHandler : IRequestHandler<GetApiKeysQuery, List<ApiKeyListItem>>
{
    private readonly AppDbContext _db;

    public GetApiKeysQueryHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<ApiKeyListItem>> Handle(GetApiKeysQuery request, CancellationToken cancellationToken)
    {
        var keys = await _db.ApiKeys
            .Where(k => k.UserId == request.UserId)
            .OrderByDescending(k => k.CreatedAt)
            .ToListAsync(cancellationToken);

        return keys.Select(k => new ApiKeyListItem(
            k.Id,
            k.Name,
            k.KeyPrefix,
            k.Status.ToString(),
            k.Scopes,
            k.LastUsedAt,
            k.ExpiresAt,
            k.CreatedAt
        )).ToList();
    }
}
