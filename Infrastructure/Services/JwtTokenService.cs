using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Services;

/// <summary>
/// Tạo và quản lý JWT access token + refresh token.
/// Đặt ở Infrastructure vì phụ thuộc vào thư viện JWT bên ngoài.
/// </summary>
public sealed class JwtTokenService(IConfiguration configuration) : IJwtTokenService
{
    private readonly string _secret = configuration["JwtSettings:Secret"]
        ?? throw new InvalidOperationException("JwtSettings:Secret is not configured.");
    private readonly string _issuer = configuration["JwtSettings:Issuer"]!;
    private readonly string _audience = configuration["JwtSettings:Audience"]!;
    private readonly int _expiryMinutes =
        int.TryParse(configuration["JwtSettings:ExpiryMinutes"], out var m) ? m : 60;

    public string GenerateAccessToken(User user, IReadOnlyList<string>? permissions = null)
    {
        var key         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Đặt cả 2 claim types để tương thích với cả raw JWT và ASP.NET Core identity
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub,        user.Id.ToString()),
            new(JwtRegisteredClaimNames.Jti,        Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.UniqueName, user.Username),
            new(JwtRegisteredClaimNames.Email,      user.Email),
            new(ClaimTypes.NameIdentifier,          user.Id.ToString()),
            new(ClaimTypes.Name,                    user.Username),
            new(ClaimTypes.Role,                    user.Role.ToString())
        };

        if (permissions is { Count: > 0 })
            foreach (var permission in permissions)
                claims.Add(new Claim("permissions", permission));

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: GetAccessTokenExpiry(),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        // Cryptographically secure random — không đoán được
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }

    public DateTime GetAccessTokenExpiry()
        => DateTime.UtcNow.AddMinutes(_expiryMinutes);
}
