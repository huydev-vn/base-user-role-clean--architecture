using Domain.Entities;
using Domain.Enums;

namespace Domain.Interfaces.Repositories;

/// <summary>
/// Repository cho RolePermission — ánh xạ Role → Permission.
/// Kế thừa toàn bộ CRUD từ IGenericRepository.
/// </summary>
public interface IRolePermissionRepository : IGenericRepository<RolePermission>
{
    /// <summary>
    /// Lấy tất cả RolePermission của một role (kèm navigation Permission).
    /// Dùng khi cần hiển thị chi tiết permissions của role trong admin UI.
    /// </summary>
    Task<IReadOnlyList<RolePermission>> GetByRoleAsync(
        UserRole role,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy danh sách Permission.Name của một role — optimized query, không load entity.
    /// Dùng cho authorization check và JWT claim generation.
    /// Chỉ trả về permissions của role đang IsActive = true.
    /// </summary>
    Task<IReadOnlyList<string>> GetPermissionNamesByRoleAsync(
        UserRole role,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Kiểm tra mapping Role → Permission đã tồn tại chưa (kể cả IsDeleted = true).
    /// Dùng trước khi Assign() để tránh duplicate.
    /// </summary>
    Task<bool> ExistsAsync(
        UserRole role,
        Guid permissionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Tìm một RolePermission cụ thể theo role và permissionId.
    /// Dùng để load entity trước khi gọi Revoke().
    /// Chỉ trả về record chưa bị soft delete (IsDeleted = false).
    /// </summary>
    Task<RolePermission?> GetByRoleAndPermissionAsync(
        UserRole role,
        Guid permissionId,
        CancellationToken cancellationToken = default);
}
