using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using backend.Dtos;
using backend.Services;

namespace backend.Endpoints
{
    public static class KnowledgeBaseEndpoints
    {
        public static WebApplication MapKnowledgeBaseEndpoints(this WebApplication app)
        {
            app.MapGet("/api/knowledge-bases", async (HttpContext context, KnowledgeBaseService service) =>
            {
                if (!context.Items.TryGetValue("UserId", out var userIdObj) || userIdObj is not Guid userId)
                    return Results.Unauthorized();

                var kbs = await service.ListAsync(userId);
                return Results.Ok(kbs);
            });

            app.MapGet("/api/knowledge-bases/{id:guid}", async (Guid id, KnowledgeBaseService service) =>
            {
                var kb = await service.GetByIdAsync(id);
                return kb == null ? Results.NotFound() : Results.Ok(kb);
            });

            app.MapPost("/api/knowledge-bases", async (HttpContext context,
                CreateKnowledgeBaseRequest request, KnowledgeBaseService service) =>
            {
                if (!context.Items.TryGetValue("UserId", out var userIdObj) || userIdObj is not Guid userId)
                    return Results.Unauthorized();

                var kb = await service.CreateAsync(userId, request);
                return Results.Created($"/api/knowledge-bases/{kb.Id}", kb);
            });

            app.MapPut("/api/knowledge-bases/{id:guid}", async (Guid id,
                UpdateKnowledgeBaseRequest request, KnowledgeBaseService service) =>
            {
                var kb = await service.UpdateAsync(id, request);
                return kb == null ? Results.NotFound() : Results.Ok(kb);
            });

            app.MapDelete("/api/knowledge-bases/{id:guid}", async (Guid id, KnowledgeBaseService service) =>
            {
                var deleted = await service.DeleteAsync(id);
                return deleted ? Results.Ok(new { message = "Knowledge base deleted" }) : Results.NotFound();
            });

            app.MapGet("/api/knowledge-bases/{knowledgeBaseId:guid}/documents", async (
                Guid knowledgeBaseId, KnowledgeBaseService service) =>
            {
                var docs = await service.ListDocumentsAsync(knowledgeBaseId);
                return Results.Ok(docs);
            });

            app.MapGet("/api/knowledge-documents/{documentId:guid}", async (Guid documentId,
                KnowledgeBaseService service) =>
            {
                var doc = await service.GetDocumentAsync(documentId);
                return doc == null ? Results.NotFound() : Results.Ok(doc);
            });

            app.MapPost("/api/knowledge-bases/{knowledgeBaseId:guid}/documents", async (
                Guid knowledgeBaseId, UploadDocumentRequest request, KnowledgeBaseService service) =>
            {
                var doc = await service.UploadDocumentAsync(knowledgeBaseId, request);
                return Results.Created($"/api/knowledge-documents/{doc.Id}", doc);
            });

            app.MapDelete("/api/knowledge-documents/{documentId:guid}", async (Guid documentId,
                KnowledgeBaseService service) =>
            {
                var deleted = await service.DeleteDocumentAsync(documentId);
                return deleted ? Results.Ok(new { message = "Document deleted" }) : Results.NotFound();
            });

            app.MapPost("/api/knowledge-documents/{documentId:guid}/chunks", async (
                Guid documentId, CreateChunkRequest request, KnowledgeBaseService service) =>
            {
                var chunk = await service.CreateChunkAsync(documentId, request);
                return Results.Created($"/api/knowledge-chunks/{chunk.Id}", chunk);
            });

            app.MapDelete("/api/knowledge-chunks/{chunkId:guid}", async (Guid chunkId,
                KnowledgeBaseService service) =>
            {
                var deleted = await service.DeleteChunkAsync(chunkId);
                return deleted ? Results.Ok(new { message = "Chunk deleted" }) : Results.NotFound();
            });

            app.MapPost("/api/knowledge-bases/{knowledgeBaseId:guid}/search", async (
                Guid knowledgeBaseId, SearchRequest request, KnowledgeBaseService service) =>
            {
                var results = await service.SearchAsync(knowledgeBaseId, request.Query, request.TopK ?? 5);
                return Results.Ok(results);
            });

            app.MapGet("/api/personas/{personaId:guid}/knowledge-bases", async (Guid personaId,
                KnowledgeBaseService service) =>
            {
                var kbs = await service.GetPersonaKnowledgeBasesAsync(personaId);
                return Results.Ok(kbs);
            });

            app.MapPost("/api/personas/{personaId:guid}/knowledge-bases/{knowledgeBaseId:guid}", async (
                Guid personaId, Guid knowledgeBaseId, KnowledgeBaseService service) =>
            {
                var linked = await service.LinkPersonaToKnowledgeBaseAsync(personaId, knowledgeBaseId);
                return linked
                    ? Results.Ok(new { message = "Knowledge base linked to persona" })
                    : Results.Conflict(new { error = "Already linked" });
            });

            app.MapDelete("/api/personas/{personaId:guid}/knowledge-bases/{knowledgeBaseId:guid}", async (
                Guid personaId, Guid knowledgeBaseId, KnowledgeBaseService service) =>
            {
                var unlinked = await service.UnlinkPersonaFromKnowledgeBaseAsync(personaId, knowledgeBaseId);
                return unlinked
                    ? Results.Ok(new { message = "Knowledge base unlinked from persona" })
                    : Results.NotFound();
            });

            return app;
        }
    }
}