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

namespace backend.Modules.CallOperations.Features.CallTransfers.AcceptTransfer;

public record AcceptTransferCommand(Guid TransferId, Guid HumanAgentId) : IRequest<CallTransferDto?>;

public class AcceptTransferCommandHandler : IRequestHandler<AcceptTransferCommand, CallTransferDto?>
{
    private readonly CallTransferService _service;

    public AcceptTransferCommandHandler(CallTransferService service)
    {
        _service = service;
    }

    public async Task<CallTransferDto?> Handle(AcceptTransferCommand request, CancellationToken cancellationToken)
    {
        return await _service.AcceptTransferAsync(request.TransferId, request.HumanAgentId);
    }
}
