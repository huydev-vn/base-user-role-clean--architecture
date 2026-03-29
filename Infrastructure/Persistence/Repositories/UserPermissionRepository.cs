using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation cho UserPermission entity.
/// Cung cấp các query đặc thù để quản lý permission overrides cấp độ user.
/// </summary>
public sealed class UserPermissionRepository(AppDbContext context)
    : GenericRepository<UserPermission>(context), IUserPermissionRepository
{
    /// <summary>
    /// Lấy tất cả UserPermission của một user kèm navigation Permission.
    /// Dùng khi hiển thị permission overrides của user trong admin UI.
    /// </summary>
    public async Task<IReadOnlyList<UserPermission>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId không được rỗng.", nameof(userId));

        var userPermissions = await _dbSet
            .Where(up => up.UserId == userId)
            .Include(up => up.Permission)
            .ToListAsync(cancellationToken);

        return userPermissions.AsReadOnly();
    }

    /// <summary>
    /// Tìm một UserPermission cụ thể theo userId và permissionId.
    /// Dùng để load entity trước khi gọi Update() hoặc soft-delete.
    /// </summary>
    public async Task<UserPermission?> GetByUserAndPermissionAsync(
        Guid userId,
        Guid permissionId,
        CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId không được rỗng.", nameof(userId));

        if (permissionId == Guid.Empty)
            throw new ArgumentException("PermissionId không được rỗng.", nameof(permissionId));

        return await _dbSet
            .Include(up => up.Permission)
            .FirstOrDefaultAsync(
                up => up.UserId == userId && up.PermissionId == permissionId,
                cancellationToken);
    }

    /// <summary>
    /// Kiểm tra một UserPermission đã tồn tại chưa.
    /// Trả về true ngay cả khi record bị soft delete (IsDeleted = true).
    /// Dùng trước khi Grant() hoặc Deny() để tránh duplicate.
    /// </summary>
    public async Task<bool> ExistsAsync(
        Guid userId,
        Guid permissionId,
        CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId không được rỗng.", nameof(userId));

        if (permissionId == Guid.Empty)
            throw new ArgumentException("PermissionId không được rỗng.", nameof(permissionId));

        return await _dbSet
            .IgnoreQueryFilters()  // Include soft-deleted records
            .AnyAsync(up => up.UserId == userId && up.PermissionId == permissionId, cancellationToken);
    }

    /// <summary>
    /// Lấy effective permission names cho một user sau khi áp dụng overrides.
    ///
    /// Thuật toán:
    ///   1. Lấy tất cả permissions của <paramref name="role"/> từ RolePermission (IsDeleted=false, IsActive=true).
    ///   2. Loại bỏ permissions user Deny (UserPermission với IsGranted=false).
    ///   3. Bổ sung permissions user Grant (UserPermission với IsGranted=true).
    ///
    /// Kết quả: là final set permissions user có thể sử dụng.
    /// Optimized: thực hiện trong một single DB query.
    /// Dùng khi generate JWT claims hoặc check authorization.
    /// </summary>
    public async Task<IReadOnlyList<string>> GetEffectivePermissionNamesAsync(
        Guid userId,
        UserRole role,
        CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId không được rỗng.", nameof(userId));

        // Bước 1: Tất cả permissions của role
        var rolePermissionNames = await context.Set<RolePermission>()
            .Where(rp => rp.Role == role && !rp.IsDeleted && rp.Permission.IsActive)
            .Select(rp => rp.Permission.Name)
            .ToListAsync(cancellationToken);

        // Bước 2: Permissions user bị deny
        var deniedPermissionNames = await _dbSet
            .Where(up => up.UserId == userId && !up.IsDeleted && !up.IsGranted)
            .Select(up => up.Permission.Name)
            .ToListAsync(cancellationToken);

        // Bước 3: Permissions user bị grant
        var grantedPermissionNames = await _dbSet
            .Where(up => up.UserId == userId && !up.IsDeleted && up.IsGranted)
            .Select(up => up.Permission.Name)
            .ToListAsync(cancellationToken);

        // Bước 4: Tính toán effective set
        var effectivePermissions = rolePermissionNames
            .Except(deniedPermissionNames)
            .Union(grantedPermissionNames)
            .OrderBy(name => name)
            .ToList();

        return effectivePermissions.AsReadOnly();
    }
}
