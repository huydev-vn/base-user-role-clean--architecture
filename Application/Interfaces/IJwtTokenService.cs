using Domain.Entities;

namespace Application.Interfaces;

public interface IJwtTokenService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();

    /// <summary>
    /// Thời điểm access token hết hạn — tính từ lúc gọi (thường là UtcNow + duration).
    /// </summary>
    DateTime GetAccessTokenExpiry();
}
