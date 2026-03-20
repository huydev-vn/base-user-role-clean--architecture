using Domain.Enums;
using Domain.Events;

namespace Domain.Events.Roles;

/// <summary>
/// Raised khi một Permission được gán cho một Role.
/// Handlers BẮT BUỘC phải: invalidate role-permission cache cho role này
/// để tất cả user thuộc role nhận quyền mới ngay lập tức.
/// </summary>
public sealed record RolePermissionAssignedEvent(
    UserRole Role,
    Guid PermissionId) : DomainEvent;
