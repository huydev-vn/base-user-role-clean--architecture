using Domain.Entities;

namespace Application.Interfaces;

public interface IJwtTokenService
{
    /// <summary>
    /// Tạo JWT access token, embedding permission claims nếu được cung cấp.
    /// </summary>
    /// <param name="user">User cần tạo token.</param>
    /// <param name="permissions">
    /// Danh sách tên permissions được nhúng vào claim "permissions".
    /// Nếu null hoặc rỗng, token chỉ chứa role claim (backward-compatible).
    /// </param>
    string GenerateAccessToken(User user, IReadOnlyList<string>? permissions = null);

    string GenerateRefreshToken();

    /// <summary>Thời điểm access token hết hạn — tính từ lúc gọi (thường là UtcNow + duration).</summary>
    DateTime GetAccessTokenExpiry();
}
