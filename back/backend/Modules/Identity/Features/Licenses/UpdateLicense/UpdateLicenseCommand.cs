using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using backend.Data;
using backend.Dtos;
using backend.Models.Enums;

namespace backend.Modules.Identity.Features.Licenses.UpdateLicense;

public record UpdateLicenseCommand(Guid Id, UpdateLicenseRequest Request) : IRequest<LicenseDto?>;

public class UpdateLicenseCommandHandler : IRequestHandler<UpdateLicenseCommand, LicenseDto?>
{
    private readonly AppDbContext _db;

    public UpdateLicenseCommandHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<LicenseDto?> Handle(UpdateLicenseCommand command, CancellationToken cancellationToken)
    {
        var license = await _db.Licenses.FindAsync(new object[] { command.Id }, cancellationToken);
        if (license == null) return null;

        var request = command.Request;
        if (request.Status != null && Enum.TryParse<LicenseStatus>(request.Status, true, out var status))
            license.Status = status;
        if (request.EndsAt.HasValue)
            license.EndsAt = request.EndsAt.Value;
        if (request.LimitsJson != null)
            license.LimitsJson = request.LimitsJson;
        if (request.MetadataJson != null)
            license.MetadataJson = request.MetadataJson;
        license.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);
        return LicenseMapper.Map(license);
    }
}
