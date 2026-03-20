using Domain.Events;

namespace Domain.Events.Users;

/// <summary>
/// Raised khi User xác thực email thành công → status chuyển sang Active.
/// Handlers có thể: gửi email chào mừng, tạo dữ liệu khởi tạo...
/// </summary>
public sealed record UserActivatedEvent(
    Guid UserId,
    string Email) : DomainEvent;
