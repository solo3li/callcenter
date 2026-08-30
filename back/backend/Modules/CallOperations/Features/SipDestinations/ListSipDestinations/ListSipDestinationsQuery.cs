using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediatR;
using backend.Data;

namespace backend.Modules.CallOperations.Features.SipDestinations.ListSipDestinations;

public record SipDestinationDto(Guid Id, string Name, string? Description, string CallTo, bool IsEnabled, DateTime CreatedAt, DateTime UpdatedAt);

public record ListSipDestinationsQuery(Guid UserId) : IRequest<List<SipDestinationDto>>;

public class ListSipDestinationsQueryHandler : IRequestHandler<ListSipDestinationsQuery, List<SipDestinationDto>>
{
    private readonly AppDbContext _db;

    public ListSipDestinationsQueryHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<SipDestinationDto>> Handle(ListSipDestinationsQuery request, CancellationToken cancellationToken)
    {
        return await _db.SipDestinations
            .Where(d => d.UserId == request.UserId)
            .OrderBy(d => d.Name)
            .Select(d => new SipDestinationDto(
                d.Id,
                d.Name,
                d.Description,
                d.CallTo,
                d.IsEnabled,
                d.CreatedAt,
                d.UpdatedAt
            ))
            .ToListAsync(cancellationToken);
    }
}
