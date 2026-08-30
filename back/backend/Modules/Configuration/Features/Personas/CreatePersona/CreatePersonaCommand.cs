using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediatR;
using backend.Data;
using backend.Dtos;
using backend.Modules.Configuration.Models;

namespace backend.Modules.Configuration.Features.Personas.CreatePersona;

public record CreatePersonaCommand(Guid UserId, string Name, string? Description) : IRequest<PersonaListItem>;

public class CreatePersonaCommandHandler : IRequestHandler<CreatePersonaCommand, PersonaListItem>
{
    private readonly AppDbContext _db;

    public CreatePersonaCommandHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<PersonaListItem> Handle(CreatePersonaCommand request, CancellationToken cancellationToken)
    {
        var persona = new Persona
        {
            UserId = request.UserId,
            Name = request.Name,
            Description = request.Description,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.Personas.Add(persona);
        await _db.SaveChangesAsync(cancellationToken);

        return PersonaMapper.Map(persona);
    }
}
