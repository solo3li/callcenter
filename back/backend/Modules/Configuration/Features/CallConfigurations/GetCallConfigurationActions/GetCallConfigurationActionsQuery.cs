using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using backend.Models.Domain;
using backend.Services;

namespace backend.Modules.Configuration.Features.CallConfigurations.GetCallConfigurationActions;

public record GetCallConfigurationActionsQuery(Guid Id, Guid UserId) : IRequest<List<CallConfigurationAction>>;

public class GetCallConfigurationActionsQueryHandler : IRequestHandler<GetCallConfigurationActionsQuery, List<CallConfigurationAction>>
{
    private readonly CallConfigurationService _service;

    public GetCallConfigurationActionsQueryHandler(CallConfigurationService service)
    {
        _service = service;
    }

    public async Task<List<CallConfigurationAction>> Handle(GetCallConfigurationActionsQuery request, CancellationToken cancellationToken)
    {
        return await _service.GetActionsAsync(request.Id, request.UserId);
    }
}
