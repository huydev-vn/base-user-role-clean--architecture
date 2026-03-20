namespace Domain.Events;

/// <summary>
/// Marker interface cho tất cả domain events.
/// </summary>
public interface IDomainEvent
{
    Guid EventId { get; }
    DateTime OccurredOn { get; }
}
