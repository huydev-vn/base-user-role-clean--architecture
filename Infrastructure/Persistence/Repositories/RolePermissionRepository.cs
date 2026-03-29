using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation cho RolePermission entity.
/// Cung cấp các query đặc thù để quản lý mapping Role → Permission.
/// </summary>
public sealed class RolePermissionRepository(AppDbContext context)
    : GenericRepository<RolePermission>(context), IRolePermissionRepository
{
    /// <summary>
    /// Lấy tất cả RolePermission của một role kèm navigation Permission.
    /// Dùng khi hiển thị chi tiết permissions của role trong admin UI.
    /// </summary>
    public async Task<IReadOnlyList<RolePermission>> GetByRoleAsync(
        UserRole role,
        CancellationToken cancellationToken = default)
    {
        var rolePermissions = await _dbSet
            .Where(rp => rp.Role == role)
            .Include(rp => rp.Permission)
            .ToListAsync(cancellationToken);

        return rolePermissions.AsReadOnly();
    }

    /// <summary>
    /// Lấy danh sách Permission.Name của một role — optimized query.
    /// Không load full entity, chỉ lấy Name — dùng cho authorization checks.
    /// Chỉ trả về permissions của role mà permission đó IsActive = true.
    /// </summary>
    public async Task<IReadOnlyList<string>> GetPermissionNamesByRoleAsync(
        UserRole role,
        CancellationToken cancellationToken = default)
    {
        var names = await _dbSet
            .Where(rp => rp.Role == role && !rp.IsDeleted && rp.Permission.IsActive)
            .Select(rp => rp.Permission.Name)
            .ToListAsync(cancellationToken);

        return names.AsReadOnly();
    }

    /// <summary>
    /// Kiểm tra mapping Role → Permission đã tồn tại chưa.
    /// Trả về true ngay cả khi record bị soft delete (IsDeleted = true).
    /// Dùng trước khi Assign() để tránh duplicate.
    /// </summary>
    public async Task<bool> ExistsAsync(
        UserRole role,
        Guid permissionId,
        CancellationToken cancellationToken = default)
    {
        if (permissionId == Guid.Empty)
            throw new ArgumentException("PermissionId không được rỗng.", nameof(permissionId));

        return await _dbSet
            .IgnoreQueryFilters()  // Include soft-deleted records
            .AnyAsync(rp => rp.Role == role && rp.PermissionId == permissionId, cancellationToken);
    }

    /// <summary>
    /// Tìm một RolePermission cụ thể theo role và permissionId.
    /// Dùng để load entity trước khi gọi Revoke().
    /// Chỉ trả về record chưa bị soft delete (IsDeleted = false).
    /// </summary>
    public async Task<RolePermission?> GetByRoleAndPermissionAsync(
        UserRole role,
        Guid permissionId,
        CancellationToken cancellationToken = default)
    {
        if (permissionId == Guid.Empty)
            throw new ArgumentException("PermissionId không được rỗng.", nameof(permissionId));

        return await _dbSet
            .Include(rp => rp.Permission)
            .FirstOrDefaultAsync(
                rp => rp.Role == role && rp.PermissionId == permissionId,
                cancellationToken);
    }
}
