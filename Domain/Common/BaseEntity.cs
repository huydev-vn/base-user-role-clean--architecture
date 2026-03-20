using Domain.Events;

namespace Domain.Common;

/// <summary>
/// Base cho tất cả entity.
/// Hỗ trợ Domain Events — cho phép entity thông báo "điều gì đó đã xảy ra"
/// mà không cần gọi trực tiếp service khác (decoupling).
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();

    private readonly List<IDomainEvent> _domainEvents = [];

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void RaiseDomainEvent(IDomainEvent domainEvent)
        => _domainEvents.Add(domainEvent);

    public void ClearDomainEvents()
        => _domainEvents.Clear();
}
