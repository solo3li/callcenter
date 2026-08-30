using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using backend.Modules.Identity.Dtos;
using backend.Modules.Billing.Dtos;
using backend.Modules.CallOperations.Dtos;
using backend.Modules.Configuration.Dtos;
using backend.Modules.Analytics.Dtos;
using backend.Models.Domain;
using backend.Services;

namespace backend.Modules.Configuration.Features.Actions.UpdateAction;

public record UpdateActionCommand(Guid Id, UpdateActionRequest Request) : IRequest<ActionDefinitionDto?>;

public class UpdateActionCommandHandler : IRequestHandler<UpdateActionCommand, ActionDefinitionDto?>
{
    private readonly ActionService _service;

    public UpdateActionCommandHandler(ActionService service)
    {
        _service = service;
    }

    public async Task<ActionDefinitionDto?> Handle(UpdateActionCommand request, CancellationToken cancellationToken)
    {
        return await _service.UpdateAsync(request.Id, request.Request);
    }
}
