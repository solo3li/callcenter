using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediatR;
using backend.Data;
using backend.Dtos;

namespace backend.Modules.Configuration.Features.Personas.GetPersona;

public record GetPersonaQuery(Guid PersonaId, Guid UserId) : IRequest<PersonaListItem?>;

public class GetPersonaQueryHandler : IRequestHandler<GetPersonaQuery, PersonaListItem?>
{
    private readonly AppDbContext _db;

    public GetPersonaQueryHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<PersonaListItem?> Handle(GetPersonaQuery request, CancellationToken cancellationToken)
    {
        var persona = await _db.Personas
            .FirstOrDefaultAsync(p => p.Id == request.PersonaId && p.UserId == request.UserId && p.IsActive, cancellationToken);

        return persona is null ? null : PersonaMapper.Map(persona);
    }
}
