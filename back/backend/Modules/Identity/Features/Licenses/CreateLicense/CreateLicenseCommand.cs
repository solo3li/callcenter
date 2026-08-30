using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using backend.Data;
using backend.Dtos;
using backend.Modules.Identity.Models;
using backend.Models.Enums;

namespace backend.Modules.Identity.Features.Licenses.CreateLicense;

public record CreateLicenseCommand(CreateLicenseRequest Request) : IRequest<LicenseDto>;

public class CreateLicenseCommandHandler : IRequestHandler<CreateLicenseCommand, LicenseDto>
{
    private readonly AppDbContext _db;

    public CreateLicenseCommandHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<LicenseDto> Handle(CreateLicenseCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var license = new License
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            PartnerId = request.PartnerId,
            PartnerPlanId = request.PartnerPlanId,
            Status = LicenseStatus.Active,
            StartsAt = request.StartsAt,
            EndsAt = request.EndsAt,
            LimitsJson = request.LimitsJson,
            MetadataJson = request.MetadataJson,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.Licenses.Add(license);
        await _db.SaveChangesAsync(cancellationToken);
        return LicenseMapper.Map(license);
    }
}
