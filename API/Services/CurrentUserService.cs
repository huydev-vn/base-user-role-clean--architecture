using System.Security.Claims;
using Application.Interfaces;

namespace API.Services;

/// <summary>
/// Lấy thông tin user hiện tại từ JWT token qua IHttpContextAccessor.
/// Đặt ở API layer vì đây là nơi duy nhất biết về HttpContext.
/// </summary>
public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    private readonly ClaimsPrincipal? _user = httpContextAccessor.HttpContext?.User;

    public string? UserId
        => _user?.FindFirstValue(ClaimTypes.NameIdentifier);

    public string? UserName
        => _user?.FindFirstValue(ClaimTypes.Name)
           ?? _user?.FindFirstValue(ClaimTypes.Email);

    public bool IsAuthenticated
        => _user?.Identity?.IsAuthenticated ?? false;
}
