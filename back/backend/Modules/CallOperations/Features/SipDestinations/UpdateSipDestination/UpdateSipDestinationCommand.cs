using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediatR;
using backend.Data;
using backend.Modules.CallOperations.Features.SipDestinations.ListSipDestinations;

namespace backend.Modules.CallOperations.Features.SipDestinations.UpdateSipDestination;

public record UpdateSipDestinationRequest(string? Name, string? CallTo, string? Description, bool? IsEnabled);

public record UpdateSipDestinationCommand(Guid Id, Guid UserId, UpdateSipDestinationRequest Request) : IRequest<SipDestinationDto?>;

public class UpdateSipDestinationCommandHandler : IRequestHandler<UpdateSipDestinationCommand, SipDestinationDto?>
{
    private readonly AppDbContext _db;

    public UpdateSipDestinationCommandHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<SipDestinationDto?> Handle(UpdateSipDestinationCommand request, CancellationToken cancellationToken)
    {
        var destination = await _db.SipDestinations
            .FirstOrDefaultAsync(d => d.Id == request.Id && d.UserId == request.UserId, cancellationToken);
            
        if (destination == null) return null;

        if (request.Request.Name != null)
        {
            var clash = await _db.SipDestinations.AnyAsync(d =>
                d.UserId == request.UserId && d.Id != request.Id &&
                d.Name.ToLower() == request.Request.Name.Trim().ToLower(), cancellationToken);
                
            if (clash) throw new InvalidOperationException("Name already in use");
            destination.Name = request.Request.Name.Trim();
        }
        
        if (request.Request.CallTo != null) destination.CallTo = request.Request.CallTo.Trim();
        if (request.Request.Description != null) destination.Description = request.Request.Description.Trim();
        if (request.Request.IsEnabled.HasValue) destination.IsEnabled = request.Request.IsEnabled.Value;
        
        destination.UpdatedAt = DateTime.UtcNow;
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
