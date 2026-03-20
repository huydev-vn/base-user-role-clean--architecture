using Domain.Events;

namespace Domain.Events.Permissions;

/// <summary>
/// Raised khi Permission mới được tạo trong hệ thống.
/// Handlers có thể: ghi audit log, sync cache, thông báo admin...
/// </summary>
public sealed record PermissionCreatedEvent(
    Guid PermissionId,
    string Name,
    string Group) : DomainEvent;
