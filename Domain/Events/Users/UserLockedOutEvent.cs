using Domain.Events;

namespace Domain.Events.Users;

/// <summary>
/// Raised khi tài khoản bị khóa do đăng nhập sai quá nhiều lần.
/// Handlers có thể: gửi email cảnh báo bảo mật cho user.
/// </summary>
public sealed record UserLockedOutEvent(
    Guid UserId,
    string Email,
    DateTime LockedUntil) : DomainEvent;
