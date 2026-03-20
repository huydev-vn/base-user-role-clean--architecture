using Domain.Common;
using Domain.Events.Users;

namespace Domain.Entities;

/// <summary>
/// Override permission ở cấp độ user cụ thể — áp dụng SAU khi tính permissions từ Role.
///
/// Hai trường hợp sử dụng:
///   IsGranted = true  → Explicit Grant: cấp thêm quyền ngoài những gì Role ban cho.
///                       Ví dụ: User có Role=Moderator nhưng cần thêm "Reports.Export".
///
///   IsGranted = false → Explicit Deny: thu hồi quyền dù Role có quyền đó.
///                       Ví dụ: Admin bị tạm block "Users.Delete" trong khi điều tra.
///
/// Thuật toán tính Effective Permissions (thực hiện ở Application layer):
///   1. Lấy tất cả permissions của User.Role từ RolePermission.
///   2. Loại bỏ những permissions có UserPermission(UserId, IsGranted=false).
///   3. Bổ sung những permissions có UserPermission(UserId, IsGranted=true).
///
/// Ưu tiên: UserPermission LUÔN override RolePermission.
/// </summary>
public sealed class UserPermission : AuditableEntity
{
    // ── Properties ────────────────────────────────────────────────────────

    /// <summary>FK tới User được áp dụng override.</summary>
    public Guid UserId { get; private set; }

    /// <summary>FK tới Permission được override.</summary>
    public Guid PermissionId { get; private set; }

    /// <summary>
    /// true  = Explicit Grant — thêm quyền ngoài role.
    /// false = Explicit Deny  — thu hồi quyền dù role có.
    /// </summary>
    public bool IsGranted { get; private set; }

    // ── Navigation ────────────────────────────────────────────────────────

    /// <summary>User được áp dụng override — loaded qua EF Include.</summary>
    public User User { get; private set; } = null!;

    /// <summary>Permission được override — loaded qua EF Include.</summary>
    public Permission Permission { get; private set; } = null!;

    // EF Core cần constructor không tham số
    private UserPermission() { }

    // ── Factory ───────────────────────────────────────────────────────────

    /// <summary>
    /// Cấp thêm permission cho user vượt ngoài role của họ.
    /// Application layer cần gọi ExistsAsync() trước để đảm bảo không duplicate.
    /// </summary>
    public static UserPermission Grant(Guid userId, Guid permissionId)
    {
        var up = Build(userId, permissionId, isGranted: true);
        up.RaiseDomainEvent(new UserPermissionGrantedEvent(userId, permissionId));
        return up;
    }

    /// <summary>
    /// Thu hồi permission của user dù role của họ có quyền đó.
    /// Application layer cần gọi ExistsAsync() trước để đảm bảo không duplicate.
    /// </summary>
    public static UserPermission Deny(Guid userId, Guid permissionId)
    {
        var up = Build(userId, permissionId, isGranted: false);
        up.RaiseDomainEvent(new UserPermissionDeniedEvent(userId, permissionId));
        return up;
    }

    // ── Domain Methods ────────────────────────────────────────────────────

    /// <summary>
    /// Cập nhật trạng thái Grant ↔ Deny.
    /// Dùng khi Admin đổi ý mà không muốn xóa và tạo lại record.
    /// Idempotent: không raise event nếu giá trị không đổi.
    /// </summary>
    public void Update(bool isGranted)
    {
        if (IsGranted == isGranted) return;

        IsGranted = isGranted;

        if (isGranted)
            RaiseDomainEvent(new UserPermissionGrantedEvent(UserId, PermissionId));
        else
            RaiseDomainEvent(new UserPermissionDeniedEvent(UserId, PermissionId));
    }

    // ── Private Helpers ───────────────────────────────────────────────────

    private static UserPermission Build(Guid userId, Guid permissionId, bool isGranted)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId không được rỗng.", nameof(userId));

        if (permissionId == Guid.Empty)
            throw new ArgumentException("PermissionId không được rỗng.", nameof(permissionId));

        return new UserPermission
        {
            UserId       = userId,
            PermissionId = permissionId,
            IsGranted    = isGranted,
        };
    }
}
