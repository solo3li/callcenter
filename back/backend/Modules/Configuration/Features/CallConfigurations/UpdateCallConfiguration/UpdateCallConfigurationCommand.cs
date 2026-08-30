using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using backend.Dtos;
using backend.Services;

namespace backend.Modules.Configuration.Features.CallConfigurations.UpdateCallConfiguration;

public record UpdateCallConfigurationCommand(Guid Id, Guid UserId, UpdateCallConfigRequest Request) : IRequest<CallConfigListItem?>;

public class UpdateCallConfigurationCommandHandler : IRequestHandler<UpdateCallConfigurationCommand, CallConfigListItem?>
{
    private readonly CallConfigurationService _service;

    public UpdateCallConfigurationCommandHandler(CallConfigurationService service)
    {
        _service = service;
    }

    public async Task<CallConfigListItem?> Handle(UpdateCallConfigurationCommand request, CancellationToken cancellationToken)
    {
        return await _service.UpdateAsync(request.Id, request.UserId, request.Request);
    }
}
