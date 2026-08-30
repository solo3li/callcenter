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

namespace backend.Modules.CallOperations.Features.CallHandoffs.AcceptCallHandoff;

public record AcceptCallHandoffCommand(Guid HandoffId) : IRequest<CallHandoffDto?>;

public class AcceptCallHandoffCommandHandler : IRequestHandler<AcceptCallHandoffCommand, CallHandoffDto?>
{
    private readonly CallHandoffService _service;

    public AcceptCallHandoffCommandHandler(CallHandoffService service)
    {
        _service = service;
    }

    public async Task<CallHandoffDto?> Handle(AcceptCallHandoffCommand request, CancellationToken cancellationToken)
    {
        return await _service.AcceptAsync(request.HandoffId);
    }
}
