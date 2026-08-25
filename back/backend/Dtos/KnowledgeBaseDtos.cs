using System;

namespace backend.Dtos
{
    public record KnowledgeBaseListItem(
        Guid Id,
        string Name,
        string? Description,
        bool IsActive,
        int DocumentCount,
        DateTime CreatedAt,
        DateTime UpdatedAt
    );

    public record CreateKnowledgeBaseRequest(
        string Name,
        string? Description
    );

    public record UpdateKnowledgeBaseRequest(
        string? Name,
        string? Description,
        bool? IsActive
    );

    public record KnowledgeDocumentDto(
        Guid Id,
        Guid KnowledgeBaseId,
        string Name,
        string SourceUri,
        string ContentType,
        string? MetadataJson,
        string Status,
        int ChunkCount,
        DateTime CreatedAt,
        DateTime UpdatedAt
    );

    public record UploadDocumentRequest(
        string Name,
        string SourceUri,
        string ContentType,
        string? MetadataJson,
        string Content
    );

    public record CreateChunkRequest(
        string Content,
        int ChunkIndex,
        string? MetadataJson
    );

    public record SearchRequest(
        string Query,
        int? TopK
    );

    public record SearchResult(
        Guid ChunkId,
        Guid DocumentId,
        string DocumentName,
        string Content,
        int ChunkIndex,
        float? Score
    );
}