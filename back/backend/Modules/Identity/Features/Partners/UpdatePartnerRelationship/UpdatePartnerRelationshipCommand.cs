using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using backend.Data;
using backend.Modules.Identity.Dtos;
using backend.Modules.Billing.Dtos;
using backend.Modules.CallOperations.Dtos;
using backend.Modules.Configuration.Dtos;
using backend.Modules.Analytics.Dtos;
using backend.Models.Enums;

namespace backend.Modules.Identity.Features.Partners.UpdatePartnerRelationship;

public record UpdatePartnerRelationshipCommand(Guid Id, string? Status, string? MetadataJson) : IRequest<PartnerRelationshipDto?>;

public class UpdatePartnerRelationshipCommandHandler : IRequestHandler<UpdatePartnerRelationshipCommand, PartnerRelationshipDto?>
{
    private readonly AppDbContext _db;

    public UpdatePartnerRelationshipCommandHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<PartnerRelationshipDto?> Handle(UpdatePartnerRelationshipCommand command, CancellationToken cancellationToken)
    {
        var rel = await _db.PartnerRelationships.FindAsync(new object[] { command.Id }, cancellationToken);
        if (rel == null) return null;

        if (command.Status != null && Enum.TryParse<PartnerRelationshipStatus>(command.Status, true, out var s))
            rel.Status = s;
        if (command.MetadataJson != null)
            rel.MetadataJson = command.MetadataJson;
        rel.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);
        return PartnerMapper.MapRelation(rel);
    }
}
