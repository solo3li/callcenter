using System;

namespace backend.Models.Domain;

public class KnowledgeDocument
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid KnowledgeBaseId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string SourceUri { get; set; } = string.Empty;
    public string ContentType { get; set; } = "text/plain";
    public string? MetadataJson { get; set; }
    public string Status { get; set; } = "pending";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public KnowledgeBase KnowledgeBase { get; set; } = null!;
    public ICollection<KnowledgeChunk> Chunks { get; set; } = new List<KnowledgeChunk>();
}