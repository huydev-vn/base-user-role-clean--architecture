using Application.Interfaces;
using Domain.Common;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Infrastructure.Persistence;

public class UnitOfWork(
    AppDbContext context,
    IDomainEventDispatcher dispatcher) : IUnitOfWork
{
    private IDbContextTransaction? _transaction;

    /// <summary>
    /// Lưu toàn bộ thay đổi pending.
    /// Trước khi save: collect tất cả domain events từ các entity đang tracked.
    /// Sau khi save thành công: dispatch domain events.
    /// </summary>
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Thu thập domain events trước khi save
        var entitiesWithEvents = context.ChangeTracker
            .Entries<BaseEntity>()
            .Where(e => e.Entity.DomainEvents.Count != 0)
            .Select(e => e.Entity)
            .ToList();

        var domainEvents = entitiesWithEvents
            .SelectMany(e => e.DomainEvents)
            .ToList();

        // Xóa events trên entity trước khi save (tránh dispatch 2 lần)
        entitiesWithEvents.ForEach(e => e.ClearDomainEvents());

        var result = await context.SaveChangesAsync(cancellationToken);

        // Dispatch sau khi save thành công
        if (domainEvents.Count > 0)
            await dispatcher.DispatchAsync(domainEvents, cancellationToken);

        return result;
    }

    /// <summary>
    /// Bắt đầu explicit transaction — chỉ dùng khi cần kiểm soát thủ công
    /// (ví dụ: gọi stored procedure xen kẽ với EF Core operations).
    /// Trường hợp thông thường chỉ cần SaveChangesAsync().
    /// </summary>
    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        => _transaction = await context.Database.BeginTransactionAsync(cancellationToken);

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is not null)
            await _transaction.CommitAsync(cancellationToken);
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is not null)
            await _transaction.RollbackAsync(cancellationToken);
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        GC.SuppressFinalize(this);
    }
}
