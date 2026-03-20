using Domain.Common;
using Domain.Enums;
using Domain.Events.Roles;

namespace Domain.Entities;

/// <summary>
/// Ánh xạ giữa một Role và một Permission — được lưu trong DB.
///
/// Thiết kế này cho phép Admin cấu hình "Role X có Permission Y"
/// tại runtime mà không cần thay đổi code hay redeploy ứng dụng.
///
/// Nguyên tắc thiết kế:
/// - Dùng Soft Delete (IsDeleted) thay vì hard delete để giữ audit trail.
/// - Gọi Revoke() thay vì xóa trực tiếp để domain event được raise.
/// - Application layer kiểm tra ExistsAsync() trước khi gọi Assign() để tránh duplicate.
/// </summary>
public sealed class RolePermission : AuditableEntity
{
    // ── Properties ────────────────────────────────────────────────────────

    /// <summary>Role được gán permission này.</summary>
    public UserRole Role { get; private set; }

    /// <summary>FK tới Permission được gán cho Role.</summary>
    public Guid PermissionId { get; private set; }

    // ── Navigation ────────────────────────────────────────────────────────

    /// <summary>Permission được gán — loaded qua EF Include.</summary>
    public Permission Permission { get; private set; } = null!;

    // EF Core cần constructor không tham số
    private RolePermission() { }

    // ── Factory ───────────────────────────────────────────────────────────

    /// <summary>
    /// Gán một Permission cho Role.
    /// Application layer cần gọi ExistsAsync() trước để đảm bảo không duplicate.
    /// </summary>
    /// <param name="role">Role nhận permission.</param>
    /// <param name="permissionId">Id của Permission được gán.</param>
    public static RolePermission Assign(UserRole role, Guid permissionId)
    {
        if (permissionId == Guid.Empty)
            throw new ArgumentException("PermissionId không được rỗng.", nameof(permissionId));

        var rolePermission = new RolePermission
        {
            Role         = role,
            PermissionId = permissionId,
        };

        rolePermission.RaiseDomainEvent(new RolePermissionAssignedEvent(role, permissionId));

        return rolePermission;
    }

    // ── Domain Methods ────────────────────────────────────────────────────

    /// <summary>
    /// Thu hồi Permission khỏi Role — soft delete + raise domain event.
    /// Handler của RolePermissionRevokedEvent cần invalidate cache cho role này.
    /// Idempotent: gọi nhiều lần không có effect phụ.
    /// </summary>
    public void Revoke()
    {
        if (IsDeleted) return;

        IsDeleted = true;
        RaiseDomainEvent(new RolePermissionRevokedEvent(Role, PermissionId));
    }
}
