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
using backend.Modules.Identity.Models;
using backend.Models.Enums;

namespace backend.Modules.Identity.Features.Partners.AddPartnerCustomer;

public record AddPartnerCustomerCommand(Guid PartnerId, CreateRelationshipRequest Request) : IRequest<PartnerRelationshipDto>;

public class AddPartnerCustomerCommandHandler : IRequestHandler<AddPartnerCustomerCommand, PartnerRelationshipDto>
{
    private readonly AppDbContext _db;

    public AddPartnerCustomerCommandHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<PartnerRelationshipDto> Handle(AddPartnerCustomerCommand command, CancellationToken cancellationToken)
    {
        var relationship = new PartnerRelationship
        {
            Id = Guid.NewGuid(),
            PartnerId = command.PartnerId,
            CustomerUserId = command.Request.CustomerUserId,
            Status = PartnerRelationshipStatus.Active,
            MetadataJson = command.Request.MetadataJson,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.PartnerRelationships.Add(relationship);
        await _db.SaveChangesAsync(cancellationToken);
        return PartnerMapper.MapRelation(relationship);
    }
}
