using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using backend.Dtos;
using backend.Services;

namespace backend.Modules.Configuration.Features.CallConfigurations.ListCallConfigurations;

public record ListCallConfigurationsQuery(Guid UserId) : IRequest<List<CallConfigListItem>>;

public class ListCallConfigurationsQueryHandler : IRequestHandler<ListCallConfigurationsQuery, List<CallConfigListItem>>
{
    private readonly CallConfigurationService _service;

    public ListCallConfigurationsQueryHandler(CallConfigurationService service)
    {
        _service = service;
    }

    public async Task<List<CallConfigListItem>> Handle(ListCallConfigurationsQuery request, CancellationToken cancellationToken)
    {
        return await _service.ListAsync(request.UserId);
    }
}
