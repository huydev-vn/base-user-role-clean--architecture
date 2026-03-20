using Domain.Events;

namespace Domain.Events.Users;

/// <summary>
/// Raised khi User cập nhật thông tin cá nhân.
/// Handlers có thể: invalidate cache, sync sang các service khác...
/// </summary>
public sealed record UserUpdatedEvent(
    Guid UserId,
    string Email,
    string FullName) : DomainEvent;
