using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using backend.Modules.Identity.Dtos;
using backend.Modules.Billing.Dtos;
using backend.Modules.CallOperations.Dtos;
using backend.Modules.Configuration.Dtos;
using backend.Modules.Analytics.Dtos;
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
