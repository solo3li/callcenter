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

namespace backend.Modules.CallOperations.Features.CallTransfers.RejectTransfer;

public record RejectTransferCommand(Guid TransferId, Guid HumanAgentId) : IRequest<TransferResponse?>;

public class RejectTransferCommandHandler : IRequestHandler<RejectTransferCommand, TransferResponse?>
{
    private readonly CallTransferService _service;

    public RejectTransferCommandHandler(CallTransferService service)
    {
        _service = service;
    }

    public async Task<TransferResponse?> Handle(RejectTransferCommand request, CancellationToken cancellationToken)
    {
        return await _service.RejectTransferAsync(request.TransferId, request.HumanAgentId);
    }
}
