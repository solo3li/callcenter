using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediatR;
using backend.Data;

namespace backend.Modules.Configuration.Features.Personas.DeletePersona;

public record DeletePersonaCommand(Guid PersonaId, Guid UserId) : IRequest<bool>;

public class DeletePersonaCommandHandler : IRequestHandler<DeletePersonaCommand, bool>
{
    private readonly AppDbContext _db;

    public DeletePersonaCommandHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<bool> Handle(DeletePersonaCommand request, CancellationToken cancellationToken)
    {
        var persona = await _db.Personas
            .FirstOrDefaultAsync(p => p.Id == request.PersonaId && p.UserId == request.UserId && p.IsActive, cancellationToken);

        if (persona is null)
            return false;

        persona.IsActive = false;
        persona.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);

        return true;
    }
}
