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

namespace backend.Modules.CallOperations.Features.CallHandoffs.GetCallHandoff;

public record GetCallHandoffQuery(Guid HandoffId) : IRequest<CallHandoffDto?>;

public class GetCallHandoffQueryHandler : IRequestHandler<GetCallHandoffQuery, CallHandoffDto?>
{
    private readonly CallHandoffService _service;

    public GetCallHandoffQueryHandler(CallHandoffService service)
    {
        _service = service;
    }

    public async Task<CallHandoffDto?> Handle(GetCallHandoffQuery request, CancellationToken cancellationToken)
    {
        return await _service.GetByIdAsync(request.HandoffId);
    }
}
