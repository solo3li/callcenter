using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using backend.Services;

namespace backend.Modules.Configuration.Features.CallConfigurations.DeleteCallConfiguration;

public record DeleteCallConfigurationCommand(Guid Id, Guid UserId) : IRequest<bool>;

public class DeleteCallConfigurationCommandHandler : IRequestHandler<DeleteCallConfigurationCommand, bool>
{
    private readonly CallConfigurationService _service;

    public DeleteCallConfigurationCommandHandler(CallConfigurationService service)
    {
        _service = service;
    }

    public async Task<bool> Handle(DeleteCallConfigurationCommand request, CancellationToken cancellationToken)
    {
        return await _service.DeleteAsync(request.Id, request.UserId);
    }
}
