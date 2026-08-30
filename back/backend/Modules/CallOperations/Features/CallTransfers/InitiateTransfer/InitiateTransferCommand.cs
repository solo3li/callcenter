using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using backend.Dtos;
using backend.Services;

namespace backend.Modules.CallOperations.Features.CallTransfers.InitiateTransfer;

public record InitiateTransferCommand(Guid CallSessionId, Guid UserId, string? TargetType, string? TargetName, string? Reason) : IRequest<object?>;

public class InitiateTransferCommandHandler : IRequestHandler<InitiateTransferCommand, object?>
{
    private readonly CallTransferService _service;

    public InitiateTransferCommandHandler(CallTransferService service)
    {
        _service = service;
    }

    public async Task<object?> Handle(InitiateTransferCommand request, CancellationToken cancellationToken)
    {
        var targetType = request.TargetType?.Trim().ToLowerInvariant();

        if (targetType == "destination")
        {
            if (string.IsNullOrWhiteSpace(request.TargetName))
                throw new InvalidOperationException("TargetName is required for destination transfers");

            var destResult = await _service.InitiateDestinationTransferAsync(
                request.CallSessionId, request.UserId, request.TargetName!, request.Reason);
            return destResult;
        }

        return await _service.InitiateTransferAsync(request.CallSessionId, request.UserId, request.Reason);
    }
}
