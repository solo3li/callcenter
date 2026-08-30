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

namespace backend.Modules.Identity.Features.Partners.UpdatePartner;

public record UpdatePartnerCommand(Guid Id, UpdatePartnerRequest Request) : IRequest<PartnerDto?>;

public class UpdatePartnerCommandHandler : IRequestHandler<UpdatePartnerCommand, PartnerDto?>
{
    private readonly AppDbContext _db;

    public UpdatePartnerCommandHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<PartnerDto?> Handle(UpdatePartnerCommand command, CancellationToken cancellationToken)
    {
        var partner = await _db.Partners.FindAsync(new object[] { command.Id }, cancellationToken);
        if (partner == null) return null;

        var request = command.Request;
        if (request.OrganizationName != null) partner.OrganizationName = request.OrganizationName;
        if (request.ContactEmail != null) partner.ContactEmail = request.ContactEmail;
        if (request.PhoneNumber != null) partner.PhoneNumber = request.PhoneNumber;
        if (request.Website != null) partner.Website = request.Website;
        if (request.Description != null) partner.Description = request.Description;
        if (request.MetadataJson != null) partner.MetadataJson = request.MetadataJson;
        partner.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);
        return PartnerMapper.Map(partner);
    }
}
