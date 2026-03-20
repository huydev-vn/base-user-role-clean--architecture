using Domain.Common;
using Domain.Events.Permissions;

namespace Domain.Entities;

/// <summary>
/// Đại diện cho một quyền hành động cụ thể trong hệ thống.
///
/// Convention đặt tên: "{Group}.{Action}"
/// Ví dụ: "Users.Read", "Users.Delete", "Reports.Export"
///
/// Nguyên tắc thiết kế:
/// - Name là immutable sau khi tạo — dùng làm policy key trong code và DB.
/// - Group tự động extract từ Name (phần trước '.' đầu tiên).
/// - Không xóa permission — chỉ Deactivate để bảo toàn audit trail và FK integrity.
/// - Mọi thay đổi trạng thái đều raise DomainEvent để handler invalidate cache.
/// </summary>
public sealed class Permission : AuditableEntity
{
    // ── Properties ────────────────────────────────────────────────────────

    /// <summary>
    /// Tên định danh duy nhất, immutable sau khi tạo.
    /// Convention: "{Group}.{Action}" — ví dụ "Users.Read", "Reports.Export".
    /// Đây chính là policy name dùng trong [Authorize(Policy = ...)].
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>Tên hiển thị thân thiện cho admin UI.</summary>
    public string DisplayName { get; private set; } = string.Empty;

    /// <summary>Mô tả chi tiết permission này cho phép làm gì.</summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Nhóm chức năng — tự động extract từ Name (phần trước '.' đầu tiên).
    /// Dùng để group permissions trong admin UI.
    /// Ví dụ: Name="Users.Read" → Group="Users".
    /// </summary>
    public string Group { get; private set; } = string.Empty;

    /// <summary>
    /// Permission có đang hoạt động không.
    /// Deactivate thay vì Delete để bảo toàn audit trail.
    /// Khi IsActive = false, tất cả policy check sẽ fail.
    /// </summary>
    public bool IsActive { get; private set; } = true;

    // ── Navigation ────────────────────────────────────────────────────────

    private readonly List<RolePermission> _rolePermissions = [];
    private readonly List<UserPermission> _userPermissions = [];

    /// <summary>Tất cả role đang được gán permission này.</summary>
    public IReadOnlyCollection<RolePermission> RolePermissions => _rolePermissions.AsReadOnly();

    /// <summary>Tất cả user-level override cho permission này.</summary>
    public IReadOnlyCollection<UserPermission> UserPermissions => _userPermissions.AsReadOnly();

    // EF Core cần constructor không tham số
    private Permission() { }

    // ── Factory ───────────────────────────────────────────────────────────

    /// <summary>
    /// Điểm duy nhất tạo Permission mới.
    /// Group được tự động extract từ Name.
    /// </summary>
    /// <param name="name">
    /// Tên định danh theo convention "{Group}.{Action}".
    /// Ví dụ: "Users.Read", "Reports.Export".
    /// Phải chứa ít nhất một dấu chấm.
    /// </param>
    /// <param name="displayName">Tên hiển thị thân thiện. Ví dụ: "Xem danh sách người dùng".</param>
    /// <param name="description">Mô tả chi tiết tùy chọn.</param>
    public static Permission Create(string name, string displayName, string? description = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);

        var trimmedName = name.Trim();

        if (!trimmedName.Contains('.'))
            throw new ArgumentException(
                $"Permission name '{trimmedName}' phải theo convention '{{Group}}.{{Action}}'. Ví dụ: 'Users.Read'.",
                nameof(name));

        var group = trimmedName[..trimmedName.IndexOf('.')];

        var permission = new Permission
        {
            Name        = trimmedName,
            DisplayName = displayName.Trim(),
            Description = description?.Trim(),
            Group       = group,
            IsActive    = true,
        };

        permission.RaiseDomainEvent(
            new PermissionCreatedEvent(permission.Id, permission.Name, permission.Group));

        return permission;
    }

    // ── Domain Methods ────────────────────────────────────────────────────

    /// <summary>
    /// Cập nhật thông tin hiển thị.
    /// Name và Group không thể thay đổi sau khi tạo.
    /// </summary>
    public void UpdateDetails(string displayName, string? description)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);

        DisplayName = displayName.Trim();
        Description = description?.Trim();

        RaiseDomainEvent(new PermissionUpdatedEvent(Id, Name));
    }

    /// <summary>
    /// Tắt permission — tất cả policy check sẽ fail, tất cả user mất quyền này.
    /// Idempotent: gọi nhiều lần không có effect phụ.
    /// </summary>
    public void Deactivate()
    {
        if (!IsActive) return;

        IsActive = false;
        RaiseDomainEvent(new PermissionUpdatedEvent(Id, Name));
    }

    /// <summary>
    /// Kích hoạt lại permission đã bị tắt.
    /// Idempotent: gọi nhiều lần không có effect phụ.
    /// </summary>
    public void Activate()
    {
        if (IsActive) return;

        IsActive = true;
        RaiseDomainEvent(new PermissionUpdatedEvent(Id, Name));
    }
}
