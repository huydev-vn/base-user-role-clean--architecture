namespace Application.DTOs.Auth;

/// <summary>
/// Trả về sau khi đăng ký thành công.
/// Không chứa token vì user chưa xác thực email — phải verify email rồi mới login.
/// </summary>
public sealed record RegisterResponse(
    Guid UserId,
    string Username,
    string Email,
    string Message);
