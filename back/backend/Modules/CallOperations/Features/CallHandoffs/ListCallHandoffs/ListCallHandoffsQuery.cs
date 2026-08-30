using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using backend.Dtos;
using backend.Services;

namespace backend.Modules.CallOperations.Features.CallHandoffs.ListCallHandoffs;

public record ListCallHandoffsQuery(Guid CallSessionId) : IRequest<List<CallHandoffDto>>;

public class ListCallHandoffsQueryHandler : IRequestHandler<ListCallHandoffsQuery, List<CallHandoffDto>>
{
    private readonly CallHandoffService _service;

    public ListCallHandoffsQueryHandler(CallHandoffService service)
    {
        _service = service;
    }

    public async Task<List<CallHandoffDto>> Handle(ListCallHandoffsQuery request, CancellationToken cancellationToken)
    {
        return await _service.ListForCallAsync(request.CallSessionId);
    }
}
