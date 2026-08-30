using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediatR;
using backend.Data;

namespace backend.Modules.CallOperations.Features.SipDestinations.DeleteSipDestination;

public record DeleteSipDestinationCommand(Guid Id, Guid UserId) : IRequest<bool>;

public class DeleteSipDestinationCommandHandler : IRequestHandler<DeleteSipDestinationCommand, bool>
{
    private readonly AppDbContext _db;

    public DeleteSipDestinationCommandHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<bool> Handle(DeleteSipDestinationCommand request, CancellationToken cancellationToken)
    {
        var destination = await _db.SipDestinations
            .FirstOrDefaultAsync(d => d.Id == request.Id && d.UserId == request.UserId, cancellationToken);
            
        if (destination == null) return false;
        
        _db.SipDestinations.Remove(destination);
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
