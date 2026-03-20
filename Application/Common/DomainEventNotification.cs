using Domain.Events;
using MediatR;

namespace Application.Common;

/// <summary>
/// Bọc một Domain Event thành MediatR INotification.
/// Cho phép Domain không phụ thuộc MediatR — Domain Events thuần túy,
/// Infrastructure dispatcher tạo wrapper này để publish qua MediatR bus.
///
/// Cách dùng:
///   Tạo handler: INotificationHandler{DomainEventNotification{UserCreatedEvent}}
/// </summary>
public record DomainEventNotification<TDomainEvent>(TDomainEvent Event)
    : INotification
    where TDomainEvent : IDomainEvent;
