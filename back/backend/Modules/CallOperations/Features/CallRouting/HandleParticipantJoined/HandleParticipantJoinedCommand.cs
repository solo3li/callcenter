using System.Threading;
using System.Threading.Tasks;
using MediatR;
using backend.Services;

namespace backend.Modules.CallOperations.Features.CallRouting.HandleParticipantJoined;

public record HandleParticipantJoinedCommand(string RoomName, string Identity) : IRequest;

public class HandleParticipantJoinedCommandHandler : IRequestHandler<HandleParticipantJoinedCommand>
{
    private readonly InboundRoutingService _routingService;

    public HandleParticipantJoinedCommandHandler(InboundRoutingService routingService)
    {
        _routingService = routingService;
    }

    public async Task Handle(HandleParticipantJoinedCommand request, CancellationToken cancellationToken)
    {
        await _routingService.HandleParticipantJoinedAsync(request.RoomName, request.Identity);
    }
}
