using Application.DTOs.Users;

namespace Application.DTOs.Auth;

/// <summary>
/// Response trả về sau đăng nhập / đăng ký thành công.
/// </summary>
public sealed record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresAt,
    UserDto User
);
