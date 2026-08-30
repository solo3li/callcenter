using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using backend.Data;
using Microsoft.EntityFrameworkCore;

namespace backend.Modules.Configuration.Features.Actions.ListActionExecutionsByCall;

public record ListActionExecutionsByCallQuery(Guid CallSessionId) : IRequest<List<object>>;

public class ListActionExecutionsByCallQueryHandler : IRequestHandler<ListActionExecutionsByCallQuery, List<object>>
{
    private readonly AppDbContext _db;

    public ListActionExecutionsByCallQueryHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<object>> Handle(ListActionExecutionsByCallQuery request, CancellationToken cancellationToken)
    {
        var executions = await _db.ActionExecutions
            .Where(e => e.CallSessionId == request.CallSessionId)
            .Include(e => e.ActionDefinition)
            .OrderByDescending(e => e.StartedAt)
            .Select(e => new
            {
                e.Id,
                e.CallSessionId,
                e.ActionDefinitionId,
                ActionDefinitionName = e.ActionDefinition.DisplayName,
                e.WorkflowExecutionId,
                Status = e.Status.ToString(),
                e.InputJson,
                e.OutputJson,
                e.Error,
                e.StartedAt,
                e.CompletedAt
            })
            .ToListAsync(cancellationToken);

        return executions.Cast<object>().ToList();
    }
}
