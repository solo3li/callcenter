using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using backend.Dtos;
using backend.Services;

namespace backend.Modules.CallOperations.Features.CallTransfers.GetCallTransfer;

public record GetCallTransferQuery(Guid CallSessionId, Guid TransferId) : IRequest<CallTransferDto?>;

public class GetCallTransferQueryHandler : IRequestHandler<GetCallTransferQuery, CallTransferDto?>
{
    private readonly CallTransferService _service;

    public GetCallTransferQueryHandler(CallTransferService service)
    {
        _service = service;
    }

    public async Task<CallTransferDto?> Handle(GetCallTransferQuery request, CancellationToken cancellationToken)
    {
        return await _service.GetByIdAsync(request.CallSessionId, request.TransferId);
    }
}
