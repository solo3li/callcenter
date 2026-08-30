using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using backend.Data;
using backend.Models.Enums;

namespace backend.Modules.Identity.Features.Licenses.DeleteLicense;

public record DeleteLicenseCommand(Guid Id) : IRequest<bool>;

public class DeleteLicenseCommandHandler : IRequestHandler<DeleteLicenseCommand, bool>
{
    private readonly AppDbContext _db;

    public DeleteLicenseCommandHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<bool> Handle(DeleteLicenseCommand command, CancellationToken cancellationToken)
    {
        var license = await _db.Licenses.FindAsync(new object[] { command.Id }, cancellationToken);
        if (license == null) return false;
        
        license.Status = LicenseStatus.Cancelled;
        license.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
