using System;
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

namespace backend.Modules.Configuration.Features.Personas.UpdatePersona;

public record UpdatePersonaCommand(Guid PersonaId, Guid UserId, string Name, string? Description, bool IsActive) : IRequest<PersonaListItem?>;

public class UpdatePersonaCommandHandler : IRequestHandler<UpdatePersonaCommand, PersonaListItem?>
{
    private readonly AppDbContext _db;

    public UpdatePersonaCommandHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<PersonaListItem?> Handle(UpdatePersonaCommand request, CancellationToken cancellationToken)
    {
        var persona = await _db.Personas
            .FirstOrDefaultAsync(p => p.Id == request.PersonaId && p.UserId == request.UserId && p.IsActive, cancellationToken);

        if (persona is null)
            return null;

        persona.Name = request.Name;
        persona.Description = request.Description;
        persona.IsActive = request.IsActive;
        persona.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);

        return PersonaMapper.Map(persona);
    }
}
