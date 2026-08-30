using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using backend.Services;

namespace backend.Modules.CallOperations.Features.CallRecordings.DeleteCallRecording;

public record DeleteCallRecordingCommand(Guid RecordingId) : IRequest<bool>;

public class DeleteCallRecordingCommandHandler : IRequestHandler<DeleteCallRecordingCommand, bool>
{
    private readonly CallRecordingService _service;

    public DeleteCallRecordingCommandHandler(CallRecordingService service)
    {
        _service = service;
    }

    public async Task<bool> Handle(DeleteCallRecordingCommand request, CancellationToken cancellationToken)
    {
        return await _service.DeleteAsync(request.RecordingId);
    }
}
