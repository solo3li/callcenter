using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using backend.Data;
using backend.Models.Enums;

namespace backend.Modules.Identity.Features.Partners.DeletePartnerRelationship;

public record DeletePartnerRelationshipCommand(Guid Id) : IRequest<bool>;

public class DeletePartnerRelationshipCommandHandler : IRequestHandler<DeletePartnerRelationshipCommand, bool>
{
    private readonly AppDbContext _db;

    public DeletePartnerRelationshipCommandHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<bool> Handle(DeletePartnerRelationshipCommand command, CancellationToken cancellationToken)
    {
        var rel = await _db.PartnerRelationships.FindAsync(new object[] { command.Id }, cancellationToken);
        if (rel == null) return false;
        
        rel.Status = PartnerRelationshipStatus.Inactive;
        rel.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
