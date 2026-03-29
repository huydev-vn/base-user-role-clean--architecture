using Domain.Enums;

namespace Application.Interfaces;

/// <summary>
/// Service tính toán effective permissions cho một user — kết hợp role permissions + user overrides.
/// Đặt ở Application layer; implementation ở Infrastructure (có thể inject cache).
/// </summary>
public interface IPermissionService
{
    /// <summary>
    /// Trả về danh sách tên permissions có hiệu lực của user:
    ///   = (RolePermissions của role) ∪ (UserGrants) ∖ (UserDenies)
    /// Kết quả được cache để tránh truy vấn DB lặp lại trong cùng request.
    /// </summary>
    Task<IReadOnlyList<string>> GetEffectivePermissionNamesAsync(
        Guid userId,
        UserRole role,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Xóa cache permissions của một user. Gọi sau mọi thay đổi RolePermission hoặc UserPermission.
    /// </summary>
    Task InvalidateCacheAsync(Guid userId, CancellationToken cancellationToken = default);
}
