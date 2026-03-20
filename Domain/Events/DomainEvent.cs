namespace Domain.Events;

/// <summary>
/// Base record cho tất cả domain events.
/// Dùng record vì domain events là immutable — không thay đổi sau khi tạo.
/// </summary>
public abstract record DomainEvent : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
