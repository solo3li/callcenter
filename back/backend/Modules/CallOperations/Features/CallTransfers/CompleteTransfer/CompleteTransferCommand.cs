using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using backend.Modules.Identity.Dtos;
using backend.Modules.Billing.Dtos;
using backend.Modules.CallOperations.Dtos;
using backend.Modules.Configuration.Dtos;
using backend.Modules.Analytics.Dtos;
using backend.Services;

namespace backend.Modules.CallOperations.Features.CallTransfers.CompleteTransfer;

public record CompleteTransferCommand(Guid TransferId) : IRequest<CallTransferDto?>;

public class CompleteTransferCommandHandler : IRequestHandler<CompleteTransferCommand, CallTransferDto?>
{
    private readonly CallTransferService _service;

    public CompleteTransferCommandHandler(CallTransferService service)
    {
        _service = service;
    }

    public async Task<CallTransferDto?> Handle(CompleteTransferCommand request, CancellationToken cancellationToken)
    {
        return await _service.CompleteTransferAsync(request.TransferId);
    }
}
