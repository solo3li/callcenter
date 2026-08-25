using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Dtos;
using backend.Models.Domain;

namespace backend.Services
{
    public class KnowledgeBaseService
    {
        private readonly AppDbContext _db;
        private readonly EmbeddingService _embeddingService;

        public KnowledgeBaseService(AppDbContext db, EmbeddingService embeddingService)
        {
            _db = db;
            _embeddingService = embeddingService;
        }

        public async Task<List<KnowledgeBaseListItem>> ListAsync(Guid userId)
        {
            var kbs = await _db.KnowledgeBases
                .Where(k => k.UserId == userId)
                .Include(k => k.Documents)
                .OrderByDescending(k => k.CreatedAt)
                .ToListAsync();

            return kbs.Select(k => new KnowledgeBaseListItem(
                k.Id, k.Name, k.Description, k.IsActive,
                k.Documents.Count, k.CreatedAt, k.UpdatedAt)).ToList();
        }

        public async Task<KnowledgeBase?> GetByIdAsync(Guid id)
        {
            return await _db.KnowledgeBases
                .Include(k => k.Documents)
                .FirstOrDefaultAsync(k => k.Id == id);
        }

        public async Task<KnowledgeBase> CreateAsync(Guid userId, CreateKnowledgeBaseRequest request)
        {
            var kb = new KnowledgeBase
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Name = request.Name,
                Description = request.Description,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _db.KnowledgeBases.Add(kb);
            await _db.SaveChangesAsync();
            return kb;
        }

        public async Task<KnowledgeBase?> UpdateAsync(Guid id, UpdateKnowledgeBaseRequest request)
        {
            var kb = await _db.KnowledgeBases.FindAsync(id);
            if (kb == null) return null;

            if (request.Name != null) kb.Name = request.Name;
            if (request.Description != null) kb.Description = request.Description;
            if (request.IsActive.HasValue) kb.IsActive = request.IsActive.Value;
            kb.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return kb;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var kb = await _db.KnowledgeBases.FindAsync(id);
            if (kb == null) return false;
            _db.KnowledgeBases.Remove(kb);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<List<KnowledgeDocumentDto>> ListDocumentsAsync(Guid knowledgeBaseId)
        {
            var docs = await _db.KnowledgeDocuments
                .Where(d => d.KnowledgeBaseId == knowledgeBaseId)
                .Include(d => d.Chunks)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();

            return docs.Select(d => new KnowledgeDocumentDto(
                d.Id, d.KnowledgeBaseId, d.Name, d.SourceUri,
                d.ContentType, d.MetadataJson, d.Status,
                d.Chunks.Count, d.CreatedAt, d.UpdatedAt)).ToList();
        }

        public async Task<KnowledgeDocumentDto> UploadDocumentAsync(
            Guid knowledgeBaseId, UploadDocumentRequest request)
        {
            var doc = new KnowledgeDocument
            {
                Id = Guid.NewGuid(),
                KnowledgeBaseId = knowledgeBaseId,
                Name = request.Name,
                SourceUri = request.SourceUri,
                ContentType = request.ContentType,
                MetadataJson = request.MetadataJson,
                Status = "processing",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _db.KnowledgeDocuments.Add(doc);

            var chunkSize = 1000;
            var content = request.Content;
            var chunkIndex = 0;

            for (var i = 0; i < content.Length; i += chunkSize)
            {
                var chunkContent = content.Substring(i, Math.Min(chunkSize, content.Length - i));
                var chunk = new KnowledgeChunk
                {
                    Id = Guid.NewGuid(),
                    KnowledgeDocumentId = doc.Id,
                    ChunkIndex = chunkIndex,
                    Content = chunkContent,
                    CreatedAt = DateTime.UtcNow
                };
                _db.KnowledgeChunks.Add(chunk);
                chunkIndex++;
            }

            doc.Status = "ready";
            await _db.SaveChangesAsync();
            return new KnowledgeDocumentDto(
                doc.Id, doc.KnowledgeBaseId, doc.Name, doc.SourceUri,
                doc.ContentType, doc.MetadataJson, doc.Status,
                chunkIndex, doc.CreatedAt, doc.UpdatedAt);
        }

        public async Task<bool> DeleteDocumentAsync(Guid documentId)
        {
            var doc = await _db.KnowledgeDocuments.FindAsync(documentId);
            if (doc == null) return false;
            _db.KnowledgeDocuments.Remove(doc);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<KnowledgeDocumentDto?> GetDocumentAsync(Guid documentId)
        {
            var d = await _db.KnowledgeDocuments
                .Include(d => d.Chunks)
                .FirstOrDefaultAsync(d => d.Id == documentId);
            if (d == null) return null;
            return new KnowledgeDocumentDto(
                d.Id, d.KnowledgeBaseId, d.Name, d.SourceUri,
                d.ContentType, d.MetadataJson, d.Status,
                d.Chunks.Count, d.CreatedAt, d.UpdatedAt);
        }

        public async Task<KnowledgeChunk> CreateChunkAsync(
            Guid documentId, CreateChunkRequest request)
        {
            var chunk = new KnowledgeChunk
            {
                Id = Guid.NewGuid(),
                KnowledgeDocumentId = documentId,
                ChunkIndex = request.ChunkIndex,
                Content = request.Content,
                MetadataJson = request.MetadataJson,
                CreatedAt = DateTime.UtcNow
            };

            _db.KnowledgeChunks.Add(chunk);
            await _db.SaveChangesAsync();
            return chunk;
        }

        public async Task<bool> DeleteChunkAsync(Guid chunkId)
        {
            var chunk = await _db.KnowledgeChunks.FindAsync(chunkId);
            if (chunk == null) return false;
            _db.KnowledgeChunks.Remove(chunk);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<List<SearchResult>> SearchAsync(Guid knowledgeBaseId, string query, int topK = 5)
        {
            var hasEmbeddings = await _db.KnowledgeChunks
                .AnyAsync(c => c.KnowledgeDocument.KnowledgeBaseId == knowledgeBaseId && c.Embedding != null);

            if (hasEmbeddings)
            {
                var embedding = await _embeddingService.GenerateEmbeddingAsync(query);
                var vectorStr = _embeddingService.FormatVectorLiteral(embedding);

                var sql = $@"
                    SELECT c.""id"", c.""knowledge_document_id"", d.""name"", c.""content"", c.""chunk_index"",
                           c.""embedding"" <=> '{vectorStr}'::vector AS similarity
                    FROM knowledge_chunks c
                    JOIN knowledge_documents d ON c.""knowledge_document_id"" = d.""id""
                    WHERE d.""knowledge_base_id"" = '{knowledgeBaseId}'
                    ORDER BY similarity
                    LIMIT {topK}";

                var results = await _db.Database.SqlQueryRaw<SearchResultRaw>(sql).ToListAsync();
                return results.Select(r => new SearchResult(r.id, r.knowledge_document_id, r.name, r.content, r.chunk_index, (float?)r.similarity)).ToList();
            }

            var chunks = await _db.KnowledgeChunks
                .Where(c => c.KnowledgeDocument.KnowledgeBaseId == knowledgeBaseId)
                .Where(c => EF.Functions.ILike(c.Content, $"%{query}%"))
                .Include(c => c.KnowledgeDocument)
                .Take(topK)
                .ToListAsync();

            return chunks.Select(c => new SearchResult(
                c.Id,
                c.KnowledgeDocumentId,
                c.KnowledgeDocument.Name,
                c.Content,
                c.ChunkIndex,
                null)).ToList();
        }

        private class SearchResultRaw
        {
            public Guid id { get; set; }
            public Guid knowledge_document_id { get; set; }
            public string name { get; set; } = "";
            public string content { get; set; } = "";
            public int chunk_index { get; set; }
            public double? similarity { get; set; }
        }

        public async Task<List<KnowledgeBaseListItem>> GetPersonaKnowledgeBasesAsync(Guid personaId)
        {
            var kbIds = await _db.PersonaKnowledgeBases
                .Where(pk => pk.PersonaId == personaId)
                .Select(pk => pk.KnowledgeBaseId)
                .ToListAsync();

            var kbs = await _db.KnowledgeBases
                .Where(k => kbIds.Contains(k.Id))
                .Include(k => k.Documents)
                .ToListAsync();

            return kbs.Select(k => new KnowledgeBaseListItem(
                k.Id, k.Name, k.Description, k.IsActive,
                k.Documents.Count, k.CreatedAt, k.UpdatedAt)).ToList();
        }

        public async Task<bool> LinkPersonaToKnowledgeBaseAsync(Guid personaId, Guid knowledgeBaseId)
        {
            var exists = await _db.PersonaKnowledgeBases
                .AnyAsync(pk => pk.PersonaId == personaId && pk.KnowledgeBaseId == knowledgeBaseId);

            if (exists) return false;

            _db.PersonaKnowledgeBases.Add(new PersonaKnowledgeBase
            {
                PersonaId = personaId,
                KnowledgeBaseId = knowledgeBaseId,
                CreatedAt = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UnlinkPersonaFromKnowledgeBaseAsync(Guid personaId, Guid knowledgeBaseId)
        {
            var pk = await _db.PersonaKnowledgeBases
                .FirstOrDefaultAsync(p => p.PersonaId == personaId && p.KnowledgeBaseId == knowledgeBaseId);

            if (pk == null) return false;

            _db.PersonaKnowledgeBases.Remove(pk);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}