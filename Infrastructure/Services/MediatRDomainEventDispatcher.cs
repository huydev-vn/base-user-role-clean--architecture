using Application.Common;
using Application.Interfaces;
using Domain.Events;
using MediatR;

namespace Infrastructure.Services;

/// <summary>
/// Dispatch Domain Events qua MediatR — thay thế LoggingDomainEventDispatcher.
///
/// Cơ chế:
///   1. Mỗi domain event được bọc trong DomainEventNotification{T}
///   2. Publish qua IPublisher (MediatR)
///   3. Các INotificationHandler{DomainEventNotification{T}} trong Application sẽ nhận và xử lý
///
/// Domain layer không biết gì về MediatR — hoàn toàn decoupled.
/// </summary>
public class MediatRDomainEventDispatcher(IPublisher publisher) : IDomainEventDispatcher
{
    public async Task DispatchAsync(
        IEnumerable<IDomainEvent> domainEvents,
        CancellationToken cancellationToken = default)
    {
        foreach (var domainEvent in domainEvents)
        {
            // Tạo DomainEventNotification<T> với T là type thực của domain event
            var notificationType = typeof(DomainEventNotification<>)
                .MakeGenericType(domainEvent.GetType());

            var notification = Activator.CreateInstance(notificationType, domainEvent);

            if (notification is INotification mediatRNotification)
                await publisher.Publish(mediatRNotification, cancellationToken);
        }
    }
}
