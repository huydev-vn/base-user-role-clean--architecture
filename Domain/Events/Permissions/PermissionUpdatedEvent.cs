using Domain.Events;

namespace Domain.Events.Permissions;

/// <summary>
/// Raised khi Permission được cập nhật details, activate hoặc deactivate.
/// Handlers BẮT BUỘC phải: invalidate permission cache để đảm bảo data nhất quán.
/// </summary>
public sealed record PermissionUpdatedEvent(
    Guid PermissionId,
    string Name) : DomainEvent;
