using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediatR;
using backend.Data;
using backend.Modules.Configuration.Models;

namespace backend.Modules.Configuration.Features.KnowledgeBases.CreateKnowledgeBase;

public record CreateKnowledgeBaseCommand(Guid UserId, string Name, string? Description) : IRequest<KnowledgeBase>;

public class CreateKnowledgeBaseCommandHandler : IRequestHandler<CreateKnowledgeBaseCommand, KnowledgeBase>
{
    private readonly AppDbContext _db;

    public CreateKnowledgeBaseCommandHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<KnowledgeBase> Handle(CreateKnowledgeBaseCommand request, CancellationToken cancellationToken)
    {
        var kb = new KnowledgeBase
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            Name = request.Name,
            Description = request.Description,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.KnowledgeBases.Add(kb);
        await _db.SaveChangesAsync(cancellationToken);
        return kb;
    }
}
