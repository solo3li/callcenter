using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediatR;
using backend.Data;
using backend.Models.Domain;
using backend.Modules.CallOperations.Features.SipDestinations.ListSipDestinations;

namespace backend.Modules.CallOperations.Features.SipDestinations.CreateSipDestination;

public record CreateSipDestinationRequest(string Name, string CallTo, string? Description);

public record CreateSipDestinationCommand(Guid UserId, CreateSipDestinationRequest Request) : IRequest<SipDestinationDto>;

public class CreateSipDestinationCommandHandler : IRequestHandler<CreateSipDestinationCommand, SipDestinationDto>
{
    private readonly AppDbContext _db;

    public CreateSipDestinationCommandHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<SipDestinationDto> Handle(CreateSipDestinationCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Request.Name) || string.IsNullOrWhiteSpace(request.Request.CallTo))
            throw new InvalidOperationException("Name and CallTo are required");

        var exists = await _db.SipDestinations
            .AnyAsync(d => d.UserId == request.UserId && d.Name.ToLower() == request.Request.Name.Trim().ToLower(), cancellationToken);
        
        if (exists)
            throw new InvalidOperationException($"Destination '{request.Request.Name}' already exists");

        var destination = new SipDestination
        {
            UserId = request.UserId,
            Name = request.Request.Name.Trim(),
            Description = request.Request.Description?.Trim(),
            CallTo = request.Request.CallTo.Trim()
        };
        
        _db.SipDestinations.Add(destination);
        await _db.SaveChangesAsync(cancellationToken);
        
        return new SipDestinationDto(
            destination.Id,
            destination.Name,
            destination.Description,
            destination.CallTo,
            destination.IsEnabled,
            destination.CreatedAt,
            destination.UpdatedAt
        );
    }
}
