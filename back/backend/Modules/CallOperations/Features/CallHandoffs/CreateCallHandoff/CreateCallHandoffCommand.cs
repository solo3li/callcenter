using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using backend.Dtos;
using backend.Services;

namespace backend.Modules.CallOperations.Features.CallHandoffs.CreateCallHandoff;

public record CreateCallHandoffCommand(Guid TransferId, string? Summary, string? ContextDataJson, string? Reason) : IRequest<CallHandoffDto?>;

public class CreateCallHandoffCommandHandler : IRequestHandler<CreateCallHandoffCommand, CallHandoffDto?>
{
    private readonly CallHandoffService _service;

    public CreateCallHandoffCommandHandler(CallHandoffService service)
    {
        _service = service;
    }

    public async Task<CallHandoffDto?> Handle(CreateCallHandoffCommand request, CancellationToken cancellationToken)
    {
        return await _service.CreateContextAsync(request.TransferId, request.Summary ?? string.Empty, request.ContextDataJson, request.Reason);
    }
}
