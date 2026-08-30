using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using backend.Models.Domain;
using backend.Dtos;
using backend.Services;

namespace backend.Modules.Configuration.Features.Actions.GetAction;

public record GetActionQuery(Guid Id) : IRequest<ActionDefinitionDto?>;

public class GetActionQueryHandler : IRequestHandler<GetActionQuery, ActionDefinitionDto?>
{
    private readonly ActionService _service;

    public GetActionQueryHandler(ActionService service)
    {
        _service = service;
    }

    public async Task<ActionDefinitionDto?> Handle(GetActionQuery request, CancellationToken cancellationToken)
    {
        return await _service.GetByIdAsync(request.Id);
    }
}
