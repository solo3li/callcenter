using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediatR;
using backend.Data;
using backend.Modules.Identity.Dtos;
using backend.Modules.Billing.Dtos;
using backend.Modules.CallOperations.Dtos;
using backend.Modules.Configuration.Dtos;
using backend.Modules.Analytics.Dtos;

namespace backend.Modules.Configuration.Features.Personas.ListPersonas;

public record ListPersonasQuery(Guid UserId) : IRequest<List<PersonaListItem>>;

public class ListPersonasQueryHandler : IRequestHandler<ListPersonasQuery, List<PersonaListItem>>
{
    private readonly AppDbContext _db;

    public ListPersonasQueryHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<PersonaListItem>> Handle(ListPersonasQuery request, CancellationToken cancellationToken)
    {
        var personas = await _db.Personas
            .Where(p => p.UserId == request.UserId && p.IsActive)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);

        return personas.Select(PersonaMapper.Map).ToList();
    }
}
