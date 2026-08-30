namespace backend.Modules.Identity.Dtos
{
    public record RegisterRequest(string Email, string Password, string DisplayName, string? CompanyName);
    public record LoginRequest(string Email, string Password);
    public record RefreshRequest(string RefreshToken);
    public record AuthResponse(string AccessToken, string RefreshToken, DateTime ExpiresAt, UserDto User);
    public record UserDto(Guid Id, string Email, string DisplayName, string? CompanyName, string Status, bool IsPartner, decimal StandardCredits, decimal PremiumCredits, DateTime CreatedAt);
    public record AgentLoginRequest(string AccessKey);
    public record AgentLoginResponse(Guid AgentId, string Name, string LivekitToken, string LivekitUrl, string OwnerUserId);
}