using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Services;

public sealed class JwtTokenService(IConfiguration configuration) : IJwtTokenService
{
    private readonly string _secret = configuration["JwtSettings:Secret"]
        ?? throw new InvalidOperationException("JwtSettings:Secret is not configured.");
    private readonly string _issuer   = configuration["JwtSettings:Issuer"]!;
    private readonly string _audience = configuration["JwtSettings:Audience"]!;
    private readonly int _expiryMinutes =
        int.TryParse(configuration["JwtSettings:ExpiryMinutes"], out var m) ? m : 15;

    public string GenerateAccessToken(User user, IReadOnlyList<string>? permissions = null)
    {
        var key         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

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
            issuer:            _issuer,
            audience:          _audience,
            claims:            claims,
            notBefore:         DateTime.UtcNow,
            expires:           GetAccessTokenExpiry(),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }

    /// <summary>
    /// SHA-256 hash của raw refresh token — chỉ hash này được lưu vào DB.
    /// Nếu DB bị breach, attacker không thể dùng hash để giả mạo token.
    /// </summary>
    public string HashRefreshToken(string rawToken)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    public DateTime GetAccessTokenExpiry()
        => DateTime.UtcNow.AddMinutes(_expiryMinutes);
}
