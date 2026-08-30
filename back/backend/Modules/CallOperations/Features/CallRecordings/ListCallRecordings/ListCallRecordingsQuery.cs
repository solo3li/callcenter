using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using backend.Modules.Identity.Dtos;
using backend.Modules.Billing.Dtos;
using backend.Modules.CallOperations.Dtos;
using backend.Modules.Configuration.Dtos;
using backend.Modules.Analytics.Dtos;
using backend.Services;

namespace backend.Modules.CallOperations.Features.CallRecordings.ListCallRecordings;

public record ListCallRecordingsQuery(Guid CallSessionId) : IRequest<List<CallRecordingDto>>;

public class ListCallRecordingsQueryHandler : IRequestHandler<ListCallRecordingsQuery, List<CallRecordingDto>>
{
    private readonly CallRecordingService _service;

    public ListCallRecordingsQueryHandler(CallRecordingService service)
    {
        _service = service;
    }

    public async Task<List<CallRecordingDto>> Handle(ListCallRecordingsQuery request, CancellationToken cancellationToken)
    {
        return await _service.ListForCallAsync(request.CallSessionId);
    }
}
