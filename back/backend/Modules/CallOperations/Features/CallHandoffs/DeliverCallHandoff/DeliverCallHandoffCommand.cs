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

namespace backend.Modules.CallOperations.Features.CallHandoffs.DeliverCallHandoff;

public record DeliverCallHandoffCommand(Guid HandoffId) : IRequest<CallHandoffDto?>;

public class DeliverCallHandoffCommandHandler : IRequestHandler<DeliverCallHandoffCommand, CallHandoffDto?>
{
    private readonly CallHandoffService _service;

    public DeliverCallHandoffCommandHandler(CallHandoffService service)
    {
        _service = service;
    }

    public async Task<CallHandoffDto?> Handle(DeliverCallHandoffCommand request, CancellationToken cancellationToken)
    {
        return await _service.DeliverAsync(request.HandoffId);
    }
}
