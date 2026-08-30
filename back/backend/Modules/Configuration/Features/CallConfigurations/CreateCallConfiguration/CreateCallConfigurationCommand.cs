using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using backend.Dtos;
using backend.Services;

namespace backend.Modules.Configuration.Features.CallConfigurations.CreateCallConfiguration;

public record CreateCallConfigurationCommand(Guid UserId, CreateCallConfigRequest Request) : IRequest<CallConfigListItem>;

public class CreateCallConfigurationCommandHandler : IRequestHandler<CreateCallConfigurationCommand, CallConfigListItem>
{
    private readonly CallConfigurationService _service;

    public CreateCallConfigurationCommandHandler(CallConfigurationService service)
    {
        _service = service;
    }

    public async Task<CallConfigListItem> Handle(CreateCallConfigurationCommand request, CancellationToken cancellationToken)
    {
        return await _service.CreateAsync(request.UserId, request.Request);
    }
}
