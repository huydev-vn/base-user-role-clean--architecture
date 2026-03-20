using Domain.Events;

namespace Domain.Events.Users;

/// <summary>
/// Raised khi user bị explicit deny một permission dù role của họ có quyền đó.
/// Handlers BẮT BUỘC phải: invalidate user-permission cache cho UserId này.
/// </summary>
public sealed record UserPermissionDeniedEvent(
    Guid UserId,
    Guid PermissionId) : DomainEvent;
