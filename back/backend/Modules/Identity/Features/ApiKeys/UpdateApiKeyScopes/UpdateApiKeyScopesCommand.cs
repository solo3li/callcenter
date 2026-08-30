using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediatR;
using backend.Data;

namespace backend.Modules.Identity.Features.ApiKeys.UpdateApiKeyScopes;

public record UpdateApiKeyScopesCommand(Guid KeyId, Guid UserId, string[] Scopes) : IRequest;

public class UpdateApiKeyScopesCommandHandler : IRequestHandler<UpdateApiKeyScopesCommand>
{
    private readonly AppDbContext _db;

    public UpdateApiKeyScopesCommandHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task Handle(UpdateApiKeyScopesCommand request, CancellationToken cancellationToken)
    {
        var key = await _db.ApiKeys.FirstOrDefaultAsync(k => k.Id == request.KeyId && k.UserId == request.UserId, cancellationToken);
        if (key == null)
            throw new InvalidOperationException("API key not found");

        key.Scopes = request.Scopes;
        await _db.SaveChangesAsync(cancellationToken);
    }
}
