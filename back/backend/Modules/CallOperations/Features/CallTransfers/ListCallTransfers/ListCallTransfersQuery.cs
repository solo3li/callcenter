using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using backend.Dtos;
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
