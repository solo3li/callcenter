using System.Threading;
using System.Threading.Tasks;
using MediatR;
using backend.Services;

namespace backend.Modules.CallOperations.Features.CallRouting.HandleParticipantLeft;

public record HandleParticipantLeftCommand(string RoomName, string Identity) : IRequest;

public class HandleParticipantLeftCommandHandler : IRequestHandler<HandleParticipantLeftCommand>
{
    private readonly InboundRoutingService _routingService;

    public HandleParticipantLeftCommandHandler(InboundRoutingService routingService)
    {
        _routingService = routingService;
    }

    public async Task Handle(HandleParticipantLeftCommand request, CancellationToken cancellationToken)
    {
        await _routingService.HandleParticipantLeftAsync(request.RoomName, request.Identity);
    }
}
