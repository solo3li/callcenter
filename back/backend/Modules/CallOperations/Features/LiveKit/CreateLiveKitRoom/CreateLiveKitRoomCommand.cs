using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using backend.Services;

namespace backend.Modules.CallOperations.Features.LiveKit.CreateLiveKitRoom;

public record CreateLiveKitRoomCommand(string RoomName) : IRequest<string>;

public class CreateLiveKitRoomCommandHandler : IRequestHandler<CreateLiveKitRoomCommand, string>
{
    private readonly LiveKitService _service;

    public CreateLiveKitRoomCommandHandler(LiveKitService service)
    {
        _service = service;
    }

    public async Task<string> Handle(CreateLiveKitRoomCommand request, CancellationToken cancellationToken)
    {
        return await _service.CreateRoom(request.RoomName);
    }
}
