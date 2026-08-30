using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using backend.Models.Domain;
using backend.Modules.Identity.Dtos;
using backend.Modules.Billing.Dtos;
using backend.Modules.CallOperations.Dtos;
using backend.Modules.Configuration.Dtos;
using backend.Modules.Analytics.Dtos;
using backend.Services;

namespace backend.Modules.Configuration.Features.Actions.ListSystemActions;

public record ListSystemActionsQuery() : IRequest<List<ActionDefinitionDto>>;

public class ListSystemActionsQueryHandler : IRequestHandler<ListSystemActionsQuery, List<ActionDefinitionDto>>
{
    private readonly ActionService _service;

    public ListSystemActionsQueryHandler(ActionService service)
    {
        _service = service;
    }

    public async Task<List<ActionDefinitionDto>> Handle(ListSystemActionsQuery request, CancellationToken cancellationToken)
    {
        return await _service.ListSystemAsync();
    }
}
