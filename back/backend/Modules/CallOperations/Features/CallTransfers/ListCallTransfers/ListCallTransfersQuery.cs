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

namespace backend.Modules.CallOperations.Features.CallTransfers.ListCallTransfers;

public record ListCallTransfersQuery(Guid CallSessionId) : IRequest<List<CallTransferDto>>;

public class ListCallTransfersQueryHandler : IRequestHandler<ListCallTransfersQuery, List<CallTransferDto>>
{
    private readonly CallTransferService _service;

    public ListCallTransfersQueryHandler(CallTransferService service)
    {
        _service = service;
    }

    public async Task<List<CallTransferDto>> Handle(ListCallTransfersQuery request, CancellationToken cancellationToken)
    {
        return await _service.ListForCallAsync(request.CallSessionId);
    }
}
