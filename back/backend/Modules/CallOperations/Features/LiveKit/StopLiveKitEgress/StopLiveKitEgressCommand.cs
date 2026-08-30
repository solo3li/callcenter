using System.Threading;
using System.Threading.Tasks;
using MediatR;
using backend.Services;

namespace backend.Modules.CallOperations.Features.LiveKit.StopLiveKitEgress;

public record StopLiveKitEgressCommand(string RoomName) : IRequest<string>;

public class StopLiveKitEgressCommandHandler : IRequestHandler<StopLiveKitEgressCommand, string>
{
    private readonly LiveKitService _service;

    public StopLiveKitEgressCommandHandler(LiveKitService service)
    {
        _service = service;
    }

    public async Task<string> Handle(StopLiveKitEgressCommand request, CancellationToken cancellationToken)
    {
        return await _service.StopEgress(request.RoomName);
    }
}
