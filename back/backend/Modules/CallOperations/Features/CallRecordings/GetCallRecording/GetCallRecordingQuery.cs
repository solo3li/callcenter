using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using backend.Dtos;
using backend.Services;

namespace backend.Modules.CallOperations.Features.CallRecordings.GetCallRecording;

public record GetCallRecordingQuery(Guid RecordingId) : IRequest<CallRecordingDto?>;

public class GetCallRecordingQueryHandler : IRequestHandler<GetCallRecordingQuery, CallRecordingDto?>
{
    private readonly CallRecordingService _service;

    public GetCallRecordingQueryHandler(CallRecordingService service)
    {
        _service = service;
    }

    public async Task<CallRecordingDto?> Handle(GetCallRecordingQuery request, CancellationToken cancellationToken)
    {
        return await _service.GetByIdAsync(request.RecordingId);
    }
}
