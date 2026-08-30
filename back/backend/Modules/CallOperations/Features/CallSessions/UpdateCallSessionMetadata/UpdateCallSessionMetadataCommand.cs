using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediatR;
using backend.Data;

namespace backend.Modules.CallOperations.Features.CallSessions.UpdateCallSessionMetadata;

public record UpdateCallSessionMetadataCommand(Guid Id, Guid UserId, string MetadataJson) : IRequest<bool>;

public class UpdateCallSessionMetadataCommandHandler : IRequestHandler<UpdateCallSessionMetadataCommand, bool>
{
    private readonly AppDbContext _db;

    public UpdateCallSessionMetadataCommandHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<bool> Handle(UpdateCallSessionMetadataCommand request, CancellationToken cancellationToken)
    {
        var session = await _db.CallSessions
            .FirstOrDefaultAsync(c => c.Id == request.Id && c.UserId == request.UserId, cancellationToken);

        if (session == null)
            return false;

        session.MetadataJson = request.MetadataJson;
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
