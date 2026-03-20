using Domain.Entities;
using Domain.Enums;

namespace Domain.Interfaces.Repositories;

/// <summary>
/// Repository cho UserPermission — override permission cấp độ user.
/// Kế thừa toàn bộ CRUD từ IGenericRepository.
/// </summary>
public interface IUserPermissionRepository : IGenericRepository<UserPermission>
{
    /// <summary>
    /// Lấy tất cả UserPermission của một user (kèm navigation Permission).
    /// Dùng khi hiển thị permission overrides của user trong admin UI.
    /// </summary>
    Task<IReadOnlyList<UserPermission>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Tìm một UserPermission cụ thể theo userId và permissionId.
    /// Dùng để load entity trước khi gọi Update() hoặc soft-delete.
    /// </summary>
    Task<UserPermission?> GetByUserAndPermissionAsync(
        Guid userId,
        Guid permissionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Kiểm tra một UserPermission đã tồn tại chưa.
    /// Dùng trước khi Grant() hoặc Deny() để tránh duplicate.
    /// </summary>
    Task<bool> ExistsAsync(
        Guid userId,
        Guid permissionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy effective permission names cho một user sau khi áp dụng overrides.
    ///
    /// Thuật toán:
    ///   1. Lấy tất cả permissions của <paramref name="role"/> (từ RolePermission).
    ///   2. Loại bỏ những permissions user Deny (IsGranted=false).
    ///   3. Bổ sung những permissions user Grant (IsGranted=true).
    ///
    /// Optimized: thực hiện trong một single DB query.
    /// Dùng khi generate JWT claims hoặc check authorization.
    /// Chỉ trả về permissions của role đang IsActive = true.
    /// </summary>
    Task<IReadOnlyList<string>> GetEffectivePermissionNamesAsync(
        Guid userId,
        UserRole role,
        CancellationToken cancellationToken = default);
}
