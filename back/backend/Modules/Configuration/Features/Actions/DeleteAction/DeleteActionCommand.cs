using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using backend.Services;

namespace backend.Modules.Configuration.Features.Actions.DeleteAction;

public record DeleteActionCommand(Guid Id) : IRequest<bool>;

public class DeleteActionCommandHandler : IRequestHandler<DeleteActionCommand, bool>
{
    private readonly ActionService _service;

    public DeleteActionCommandHandler(ActionService service)
    {
        _service = service;
    }

    public async Task<bool> Handle(DeleteActionCommand request, CancellationToken cancellationToken)
    {
        return await _service.DeleteAsync(request.Id);
    }
}
