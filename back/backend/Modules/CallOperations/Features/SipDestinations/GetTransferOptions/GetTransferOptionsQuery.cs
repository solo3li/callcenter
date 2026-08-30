using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediatR;
using backend.Data;
using backend.Models.Enums;

namespace backend.Modules.CallOperations.Features.SipDestinations.GetTransferOptions;

public record TransferOptionDto(string Type, string Name, bool Available);

public record GetTransferOptionsQuery(Guid UserId) : IRequest<GetTransferOptionsResponse>;

public record GetTransferOptionsResponse(List<TransferOptionDto> Agents, List<TransferOptionDto> Destinations);

public class GetTransferOptionsQueryHandler : IRequestHandler<GetTransferOptionsQuery, GetTransferOptionsResponse>
{
    private readonly AppDbContext _db;

    public GetTransferOptionsQueryHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<GetTransferOptionsResponse> Handle(GetTransferOptionsQuery request, CancellationToken cancellationToken)
    {
        var agents = await _db.HumanAgents
            .Where(a => a.OwnerUserId == request.UserId && a.IsActive)
            .Select(a => new TransferOptionDto("human", a.Name, a.Status == HumanAgentStatus.Available))
            .ToListAsync(cancellationToken);
            
        var destinations = await _db.SipDestinations
            .Where(d => d.UserId == request.UserId && d.IsEnabled)
            .Select(d => new TransferOptionDto("destination", d.Name, true))
            .ToListAsync(cancellationToken);
            
        return new GetTransferOptionsResponse(agents, destinations);
    }
}
