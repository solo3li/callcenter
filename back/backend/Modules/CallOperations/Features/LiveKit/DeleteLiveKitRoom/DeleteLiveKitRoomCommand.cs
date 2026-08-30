using System.Threading;
using System.Threading.Tasks;
using MediatR;
using backend.Services;

namespace backend.Modules.CallOperations.Features.LiveKit.DeleteLiveKitRoom;

public record DeleteLiveKitRoomCommand(string RoomName) : IRequest<string>;

public class DeleteLiveKitRoomCommandHandler : IRequestHandler<DeleteLiveKitRoomCommand, string>
{
    private readonly LiveKitService _service;

    public DeleteLiveKitRoomCommandHandler(LiveKitService service)
    {
        _service = service;
    }

    public async Task<string> Handle(DeleteLiveKitRoomCommand request, CancellationToken cancellationToken)
    {
        return await _service.DeleteRoom(request.RoomName);
    }
}
