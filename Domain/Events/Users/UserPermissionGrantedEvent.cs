using Domain.Events;

namespace Domain.Events.Users;

/// <summary>
/// Raised khi user được explicit grant thêm permission vượt ngoài role của họ.
/// Handlers BẮT BUỘC phải: invalidate user-permission cache cho UserId này.
/// </summary>
public sealed record UserPermissionGrantedEvent(
    Guid UserId,
    Guid PermissionId) : DomainEvent;
