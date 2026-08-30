using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using MediatR;
using backend.Modules.Identity.Dtos;
using backend.Modules.Billing.Dtos;
using backend.Modules.CallOperations.Dtos;
using backend.Modules.Configuration.Dtos;
using backend.Modules.Analytics.Dtos;
using backend.Modules.Configuration.Features.KnowledgeBases.ListKnowledgeBases;
using backend.Modules.Configuration.Features.KnowledgeBases.GetKnowledgeBase;
using backend.Modules.Configuration.Features.KnowledgeBases.CreateKnowledgeBase;
using backend.Modules.Configuration.Features.KnowledgeBases.UpdateKnowledgeBase;
using backend.Modules.Configuration.Features.KnowledgeBases.DeleteKnowledgeBase;
using backend.Modules.Configuration.Features.KnowledgeBases.ListDocuments;
using backend.Modules.Configuration.Features.KnowledgeBases.GetDocument;
using backend.Modules.Configuration.Features.KnowledgeBases.UploadDocument;
using backend.Modules.Configuration.Features.KnowledgeBases.DeleteDocument;
using backend.Modules.Configuration.Features.KnowledgeBases.CreateChunk;
using backend.Modules.Configuration.Features.KnowledgeBases.DeleteChunk;
using backend.Modules.Configuration.Features.KnowledgeBases.SearchKnowledgeBases;
using backend.Modules.Configuration.Features.KnowledgeBases.GetPersonaKnowledgeBases;
using backend.Modules.Configuration.Features.KnowledgeBases.LinkPersonaToKnowledgeBase;
using backend.Modules.Configuration.Features.KnowledgeBases.UnlinkPersonaFromKnowledgeBase;

namespace backend.Modules.Configuration.Endpoints
{
    public static class KnowledgeBaseEndpoints
    {
        public static WebApplication MapKnowledgeBaseEndpoints(this WebApplication app)
        {
            app.MapGet("/api/knowledge-bases", async (HttpContext context, IMediator mediator) =>
            {
                if (!context.Items.TryGetValue("UserId", out var userIdObj) || userIdObj is not Guid userId)
                    return Results.Unauthorized();

                var kbs = await mediator.Send(new ListKnowledgeBasesQuery(userId));
                return Results.Ok(kbs);
            });

            app.MapGet("/api/knowledge-bases/{id:guid}", async (Guid id, IMediator mediator) =>
            {
                var kb = await mediator.Send(new GetKnowledgeBaseQuery(id));
                return kb == null ? Results.NotFound() : Results.Ok(kb);
            });

            app.MapPost("/api/knowledge-bases", async (HttpContext context, CreateKnowledgeBaseRequest request, IMediator mediator) =>
            {
                if (!context.Items.TryGetValue("UserId", out var userIdObj) || userIdObj is not Guid userId)
                    return Results.Unauthorized();

                var kb = await mediator.Send(new CreateKnowledgeBaseCommand(userId, request.Name, request.Description));
                return Results.Created($"/api/knowledge-bases/{kb.Id}", kb);
            });

            app.MapPut("/api/knowledge-bases/{id:guid}", async (Guid id, UpdateKnowledgeBaseRequest request, IMediator mediator) =>
            {
                var kb = await mediator.Send(new UpdateKnowledgeBaseCommand(id, request.Name, request.Description, request.IsActive));
                return kb == null ? Results.NotFound() : Results.Ok(kb);
            });

            app.MapDelete("/api/knowledge-bases/{id:guid}", async (Guid id, IMediator mediator) =>
            {
                var deleted = await mediator.Send(new DeleteKnowledgeBaseCommand(id));
                return deleted ? Results.Ok(new { message = "Knowledge base deleted" }) : Results.NotFound();
            });

            app.MapGet("/api/knowledge-bases/{knowledgeBaseId:guid}/documents", async (Guid knowledgeBaseId, IMediator mediator) =>
            {
                var docs = await mediator.Send(new ListDocumentsQuery(knowledgeBaseId));
                return Results.Ok(docs);
            });

            app.MapGet("/api/knowledge-documents/{documentId:guid}", async (Guid documentId, IMediator mediator) =>
            {
                var doc = await mediator.Send(new GetDocumentQuery(documentId));
                return doc == null ? Results.NotFound() : Results.Ok(doc);
            });

            app.MapPost("/api/knowledge-bases/{knowledgeBaseId:guid}/documents", async (Guid knowledgeBaseId, UploadDocumentRequest request, IMediator mediator) =>
            {
                var doc = await mediator.Send(new UploadDocumentCommand(knowledgeBaseId, request.Name, request.SourceUri, request.ContentType, request.Content, request.MetadataJson));
                return Results.Created($"/api/knowledge-documents/{doc.Id}", doc);
            });

            app.MapDelete("/api/knowledge-documents/{documentId:guid}", async (Guid documentId, IMediator mediator) =>
            {
                var deleted = await mediator.Send(new DeleteDocumentCommand(documentId));
                return deleted ? Results.Ok(new { message = "Document deleted" }) : Results.NotFound();
            });

            app.MapPost("/api/knowledge-documents/{documentId:guid}/chunks", async (Guid documentId, CreateChunkRequest request, IMediator mediator) =>
            {
                var chunk = await mediator.Send(new CreateChunkCommand(documentId, request.ChunkIndex, request.Content, request.MetadataJson));
                return Results.Created($"/api/knowledge-chunks/{chunk.Id}", chunk);
            });

            app.MapDelete("/api/knowledge-chunks/{chunkId:guid}", async (Guid chunkId, IMediator mediator) =>
            {
                var deleted = await mediator.Send(new DeleteChunkCommand(chunkId));
                return deleted ? Results.Ok(new { message = "Chunk deleted" }) : Results.NotFound();
            });

            app.MapPost("/api/knowledge-bases/{knowledgeBaseId:guid}/search", async (Guid knowledgeBaseId, SearchRequest request, IMediator mediator) =>
            {
                var results = await mediator.Send(new SearchKnowledgeBasesQuery(knowledgeBaseId, request.Query, request.TopK ?? 5));
                return Results.Ok(results);
            });

            app.MapGet("/api/personas/{personaId:guid}/knowledge-bases", async (Guid personaId, IMediator mediator) =>
            {
                var kbs = await mediator.Send(new GetPersonaKnowledgeBasesQuery(personaId));
                return Results.Ok(kbs);
            });

            app.MapPost("/api/personas/{personaId:guid}/knowledge-bases/{knowledgeBaseId:guid}", async (Guid personaId, Guid knowledgeBaseId, IMediator mediator) =>
            {
                var linked = await mediator.Send(new LinkPersonaToKnowledgeBaseCommand(personaId, knowledgeBaseId));
                return linked
                    ? Results.Ok(new { message = "Knowledge base linked to persona" })
                    : Results.Conflict(new { error = "Already linked" });
            });

            app.MapDelete("/api/personas/{personaId:guid}/knowledge-bases/{knowledgeBaseId:guid}", async (Guid personaId, Guid knowledgeBaseId, IMediator mediator) =>
            {
                var unlinked = await mediator.Send(new UnlinkPersonaFromKnowledgeBaseCommand(personaId, knowledgeBaseId));
                return unlinked
                    ? Results.Ok(new { message = "Knowledge base unlinked from persona" })
                    : Results.NotFound();
            });

            return app;
        }
    }
}