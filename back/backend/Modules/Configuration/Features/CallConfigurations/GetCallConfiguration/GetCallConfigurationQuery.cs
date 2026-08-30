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

namespace backend.Modules.Configuration.Features.CallConfigurations.GetCallConfiguration;

public record GetCallConfigurationQuery(Guid Id, Guid UserId) : IRequest<CallConfigListItem?>;

public class GetCallConfigurationQueryHandler : IRequestHandler<GetCallConfigurationQuery, CallConfigListItem?>
{
    private readonly CallConfigurationService _service;

    public GetCallConfigurationQueryHandler(CallConfigurationService service)
    {
        _service = service;
    }

    public async Task<CallConfigListItem?> Handle(GetCallConfigurationQuery request, CancellationToken cancellationToken)
    {
        return await _service.GetByIdAsync(request.Id, request.UserId);
    }
}
