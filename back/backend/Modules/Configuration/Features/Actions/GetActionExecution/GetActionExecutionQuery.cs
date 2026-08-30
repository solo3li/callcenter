using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using backend.Data;
using Microsoft.EntityFrameworkCore;

namespace backend.Modules.Configuration.Features.Actions.GetActionExecution;

public record GetActionExecutionQuery(Guid Id) : IRequest<object?>;

public class GetActionExecutionQueryHandler : IRequestHandler<GetActionExecutionQuery, object?>
{
    private readonly AppDbContext _db;

    public GetActionExecutionQueryHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<object?> Handle(GetActionExecutionQuery request, CancellationToken cancellationToken)
    {
        var execution = await _db.ActionExecutions
            .Include(e => e.ActionDefinition)
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

        if (execution is null)
            return null;

        return new
        {
            execution.Id,
            execution.CallSessionId,
            execution.ActionDefinitionId,
            ActionDefinitionName = execution.ActionDefinition.DisplayName,
            execution.WorkflowExecutionId,
            Status = execution.Status.ToString(),
            execution.InputJson,
            execution.OutputJson,
            execution.Error,
            execution.StartedAt,
            execution.CompletedAt
        };
    }
}
