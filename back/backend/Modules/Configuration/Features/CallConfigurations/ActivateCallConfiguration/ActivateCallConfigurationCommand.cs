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

namespace backend.Modules.Configuration.Features.CallConfigurations.ActivateCallConfiguration;

public record ActivateCallConfigurationCommand(Guid Id, Guid UserId) : IRequest<CallConfigListItem?>;

public class ActivateCallConfigurationCommandHandler : IRequestHandler<ActivateCallConfigurationCommand, CallConfigListItem?>
{
    private readonly CallConfigurationService _service;

    public ActivateCallConfigurationCommandHandler(CallConfigurationService service)
    {
        _service = service;
    }

    public async Task<CallConfigListItem?> Handle(ActivateCallConfigurationCommand request, CancellationToken cancellationToken)
    {
        return await _service.ActivateAsync(request.Id, request.UserId);
    }
}
