using System.Threading;
using System.Threading.Tasks;
using MediatR;
using backend.Services;

namespace backend.Modules.CallOperations.Features.CallRouting.HandleRoomFinished;

public record HandleRoomFinishedCommand(string RoomName) : IRequest;

public class HandleRoomFinishedCommandHandler : IRequestHandler<HandleRoomFinishedCommand>
{
    private readonly InboundRoutingService _routingService;

    public HandleRoomFinishedCommandHandler(InboundRoutingService routingService)
    {
        _routingService = routingService;
    }

    public async Task Handle(HandleRoomFinishedCommand request, CancellationToken cancellationToken)
    {
        await _routingService.HandleRoomFinishedAsync(request.RoomName);
    }
}
