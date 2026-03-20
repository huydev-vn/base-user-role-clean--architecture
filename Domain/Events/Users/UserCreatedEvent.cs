using Domain.Events;

namespace Domain.Events.Users;

/// <summary>
/// Raised khi User đăng ký thành công.
/// Handlers có thể: gửi email xác thực, tạo wallet mặc định, log analytics...
/// </summary>
public sealed record UserCreatedEvent(
    Guid UserId,
    string Email,
    string FullName) : DomainEvent;
