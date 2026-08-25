using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using backend.Data;
using backend.Dtos;
using backend.Models.Domain;
using backend.Models.Enums;

namespace backend.Services
{
    public class AuthService
    {
        private readonly AppDbContext _db;
        private readonly string _jwtSecret;

        public AuthService(AppDbContext db)
        {
            _db = db;
            _jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET") ?? "super-secret-key-change-in-production";
        }

        public async Task<User> RegisterAsync(RegisterRequest request)
        {
            var existing = await _db.Users.AnyAsync(u => u.Email.ToLower() == request.Email.ToLower());
            if (existing)
                throw new InvalidOperationException("Email already in use");

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                DisplayName = request.DisplayName,
                CompanyName = request.CompanyName,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Status = UserStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            return user;
        }

        public async Task<string> LoginAsync(LoginRequest request)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower());
            if (user == null)
                throw new UnauthorizedAccessException("Invalid email or password");

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                throw new UnauthorizedAccessException("Invalid email or password");

            if (user.Status != UserStatus.Active)
                throw new UnauthorizedAccessException("Account is not active");

            return GenerateJwt(user);
        }

        public async Task<Guid?> ValidateTokenAsync(string token)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecret));

                var result = await handler.ValidateTokenAsync(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                });

                if (!result.IsValid)
                    return null;

                var jwt = result.SecurityToken as JwtSecurityToken;
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

        public async Task<User?> GetUserByIdAsync(Guid userId)
        {
            return await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
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
}