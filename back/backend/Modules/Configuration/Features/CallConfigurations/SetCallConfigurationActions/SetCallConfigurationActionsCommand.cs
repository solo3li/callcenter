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

namespace backend.Modules.Configuration.Features.CallConfigurations.SetCallConfigurationActions;

public record SetCallConfigurationActionsCommand(Guid Id, Guid UserId, SetConfigActionsRequest Request) : IRequest<CallConfigListItem?>;

public class SetCallConfigurationActionsCommandHandler : IRequestHandler<SetCallConfigurationActionsCommand, CallConfigListItem?>
{
    private readonly CallConfigurationService _service;

    public SetCallConfigurationActionsCommandHandler(CallConfigurationService service)
    {
        _service = service;
    }

    public async Task<CallConfigListItem?> Handle(SetCallConfigurationActionsCommand request, CancellationToken cancellationToken)
    {
        return await _service.SetActionsAsync(request.Id, request.UserId, request.Request);
    }
}
