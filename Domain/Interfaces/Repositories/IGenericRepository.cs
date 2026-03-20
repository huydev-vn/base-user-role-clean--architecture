using System.Linq.Expressions;
using Domain.Common;

namespace Domain.Interfaces.Repositories;

public interface IGenericRepository<T> where T : BaseEntity
{
    // ── Queries ──────────────────────────────────────────────────────────
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<IEnumerable<T>> FindAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default);

    Task<T?> FirstOrDefaultAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default);

    Task<int> CountAsync(
        Expression<Func<T, bool>>? predicate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Trả về IQueryable để gọi LINQ phức tạp, Include, projection...
    /// Chỉ dùng khi FindAsync không đủ. Không gọi ở tầng trên Infrastructure.
    /// </summary>
    IQueryable<T> Query();

    // ── Commands ─────────────────────────────────────────────────────────
    Task AddAsync(T entity, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
    Task UpdateAsync(T entity);
    Task DeleteAsync(Guid id);
}

