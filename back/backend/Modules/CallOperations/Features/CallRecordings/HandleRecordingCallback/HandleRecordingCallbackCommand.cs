using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using backend.Dtos;
using backend.Services;

namespace backend.Modules.CallOperations.Features.CallRecordings.HandleRecordingCallback;

public record HandleRecordingCallbackCommand(Guid CallSessionId, RecordingCallbackRequest Request) : IRequest<CallRecordingDto>;

public class HandleRecordingCallbackCommandHandler : IRequestHandler<HandleRecordingCallbackCommand, CallRecordingDto>
{
    private readonly CallRecordingService _service;

    public HandleRecordingCallbackCommandHandler(CallRecordingService service)
    {
        _service = service;
    }

    public async Task<CallRecordingDto> Handle(HandleRecordingCallbackCommand request, CancellationToken cancellationToken)
    {
        return await _service.HandleEgressCallback(request.CallSessionId, request.Request);
    }
}
