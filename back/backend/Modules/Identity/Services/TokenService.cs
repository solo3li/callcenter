using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Linq;
using Microsoft.IdentityModel.Tokens;
using backend.Modules.Identity.Models;

namespace backend.Modules.Identity.Services;

public class TokenService
{
    private readonly string _jwtSecret;

    public TokenService()
    {
        _jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET") ?? throw new InvalidOperationException("JWT_SECRET environment variable is required");
    }

    public Guid? ValidateToken(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecret));

            handler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            }, out var validatedToken);

            var jwt = validatedToken as JwtSecurityToken;
            var sub = jwt?.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            if (sub == null || !Guid.TryParse(sub, out var userId))
                return null;

            return userId;
        }
        catch
        {
            return null;
        }
    }

    public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecret));

            var parameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = false // important for refresh token
            };

            var principal = handler.ValidateToken(token, parameters, out var securityToken);
            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }

            return principal;
        }
        catch
        {
            return null;
        }
    }

    public string GenerateJwt(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var now = DateTime.UtcNow;
        var expires = now.AddHours(24);

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim("sub", user.Id.ToString()),
                new Claim("email", user.Email),
                new Claim("iat", new DateTimeOffset(now).ToUnixTimeSeconds().ToString()),
            }),
            Expires = expires,
            NotBefore = now,
            SigningCredentials = credentials
        };

        var handler = new JwtSecurityTokenHandler();
        var token = handler.CreateToken(descriptor);
        return handler.WriteToken(token);
    }
}
