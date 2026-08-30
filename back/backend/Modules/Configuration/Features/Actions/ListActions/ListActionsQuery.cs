using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using backend.Models.Domain;
using backend.Dtos;
using backend.Services;

namespace backend.Modules.Configuration.Features.Actions.ListActions;

public record ListActionsQuery(string? Type) : IRequest<List<ActionDefinitionDto>>;

public class ListActionsQueryHandler : IRequestHandler<ListActionsQuery, List<ActionDefinitionDto>>
{
    private readonly ActionService _service;

    public ListActionsQueryHandler(ActionService service)
    {
        _service = service;
    }

    public async Task<List<ActionDefinitionDto>> Handle(ListActionsQuery request, CancellationToken cancellationToken)
    {
        return await _service.ListAsync(request.Type);
    }
}
