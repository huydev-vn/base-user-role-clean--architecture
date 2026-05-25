using Domain.Entities;

namespace Application.Interfaces;

public interface IJwtTokenService
{
    /// <summary>
    /// Tạo JWT access token, embedding permission claims nếu được cung cấp.
    /// </summary>
    string GenerateAccessToken(User user, IReadOnlyList<string>? permissions = null);

    /// <summary>
    /// Sinh refresh token ngẫu nhiên an toàn (raw — trả về client, không lưu DB).
    /// </summary>
    string GenerateRefreshToken();

    /// <summary>
    /// Hash refresh token bằng SHA-256 trước khi lưu vào DB.
    /// DB chỉ chứa hash; raw token chỉ gửi cho client một lần duy nhất.
    /// </summary>
    string HashRefreshToken(string rawToken);

    /// <summary>Thời điểm access token hết hạn — tính từ lúc gọi.</summary>
    DateTime GetAccessTokenExpiry();
}
