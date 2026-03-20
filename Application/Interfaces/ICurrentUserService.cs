namespace Application.Interfaces;

/// <summary>
/// Cung cấp thông tin user đang đăng nhập cho các layer bên dưới
/// mà không cần phụ thuộc trực tiếp vào HttpContext.
/// Interface nằm ở Application, implementation nằm ở API.
/// </summary>
public interface ICurrentUserService
{
    string? UserId { get; }
    string? UserName { get; }
    bool IsAuthenticated { get; }
}
