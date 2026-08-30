namespace backend.Modules.Identity.Dtos
{
    public record ApiKeyListItem(Guid Id, string Name, string KeyPrefix, string Status, string[] Scopes, DateTime? LastUsedAt, DateTime? ExpiresAt, DateTime CreatedAt);
    public record CreateApiKeyRequest(string Name, string[]? Scopes, DateTime? ExpiresAt);
    public record CreateApiKeyResponse(Guid Id, string Name, string RawKey, string KeyPrefix, DateTime CreatedAt);
    public record UpdateApiKeyScopesRequest(string[] Scopes);
}