using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using backend.Dtos;
using backend.Services;

namespace backend.Modules.CallOperations.Features.CallRecordings.GetCallRecordingDownloadUrl;

public record GetCallRecordingDownloadUrlQuery(Guid RecordingId) : IRequest<DownloadUrlResponse>;

public class GetCallRecordingDownloadUrlQueryHandler : IRequestHandler<GetCallRecordingDownloadUrlQuery, DownloadUrlResponse>
{
    private readonly CallRecordingService _service;

    public GetCallRecordingDownloadUrlQueryHandler(CallRecordingService service)
    {
        _service = service;
    }

    public async Task<DownloadUrlResponse> Handle(GetCallRecordingDownloadUrlQuery request, CancellationToken cancellationToken)
    {
        return await _service.GenerateDownloadUrl(request.RecordingId);
    }
}
