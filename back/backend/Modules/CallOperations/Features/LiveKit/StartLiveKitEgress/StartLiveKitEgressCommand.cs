using System.Threading;
using System.Threading.Tasks;
using MediatR;
using backend.Services;

namespace backend.Modules.CallOperations.Features.LiveKit.StartLiveKitEgress;

public record StartLiveKitEgressCommand(string RoomName) : IRequest<string>;

public class StartLiveKitEgressCommandHandler : IRequestHandler<StartLiveKitEgressCommand, string>
{
    private readonly LiveKitService _service;

    public StartLiveKitEgressCommandHandler(LiveKitService service)
    {
        _service = service;
    }

    public async Task<string> Handle(StartLiveKitEgressCommand request, CancellationToken cancellationToken)
    {
        return await _service.StartEgress(request.RoomName);
    }
}
