using Domain.Events;

namespace Application.Interfaces;

/// <summary>
/// Dispatcher cho Domain Events sau khi SaveChanges thành công.
/// Implementation mặc định chỉ log — có thể swap sang MediatR,
/// RabbitMQ, hoặc bất kỳ event bus nào mà không cần đổi code business logic.
/// </summary>
public interface IDomainEventDispatcher
{
    Task DispatchAsync(
        IEnumerable<IDomainEvent> domainEvents,
        CancellationToken cancellationToken = default);
}
