using Domain.Enums;
using Domain.Events;

namespace Domain.Events.Roles;

/// <summary>
/// Raised khi một Permission bị thu hồi khỏi một Role.
/// Handlers BẮT BUỘC phải: invalidate role-permission cache cho role này
/// để tất cả user thuộc role mất quyền ngay lập tức.
/// </summary>
public sealed record RolePermissionRevokedEvent(
    UserRole Role,
    Guid PermissionId) : DomainEvent;
