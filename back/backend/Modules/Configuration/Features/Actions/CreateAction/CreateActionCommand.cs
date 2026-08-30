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

namespace backend.Modules.Configuration.Features.Actions.CreateAction;

public record CreateActionCommand(CreateActionRequest Request) : IRequest<ActionDefinitionDto>;

public class CreateActionCommandHandler : IRequestHandler<CreateActionCommand, ActionDefinitionDto>
{
    private readonly ActionService _service;

    public CreateActionCommandHandler(ActionService service)
    {
        _service = service;
    }

    public async Task<ActionDefinitionDto> Handle(CreateActionCommand request, CancellationToken cancellationToken)
    {
        return await _service.CreateAsync(request.Request);
    }
}
