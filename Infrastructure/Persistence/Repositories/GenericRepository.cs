using System.Data;
using System.Linq.Expressions;
using Domain.Common;
using Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Infrastructure.Persistence.Repositories;

public class GenericRepository<T>(AppDbContext context) : IGenericRepository<T>
    where T : BaseEntity
{
    protected readonly AppDbContext _context = context;
    protected readonly DbSet<T> _dbSet = context.Set<T>();

    // ── Queries ───────────────────────────────────────────────────────────

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _dbSet.FindAsync([id], cancellationToken);

    public async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _dbSet.ToListAsync(cancellationToken);

    public async Task<IEnumerable<T>> FindAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default)
        => await _dbSet.Where(predicate).ToListAsync(cancellationToken);

    public async Task<T?> FirstOrDefaultAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default)
        => await _dbSet.FirstOrDefaultAsync(predicate, cancellationToken);

    public async Task<bool> ExistsAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default)
        => await _dbSet.AnyAsync(predicate, cancellationToken);

    public async Task<int> CountAsync(
        Expression<Func<T, bool>>? predicate = null,
        CancellationToken cancellationToken = default)
        => predicate is null
            ? await _dbSet.CountAsync(cancellationToken)
            : await _dbSet.CountAsync(predicate, cancellationToken);

    /// <summary>
    /// Trả về IQueryable để gọi Include, projection, phân trang phức tạp.
    /// Chỉ dùng khi các method trên không đủ — không expose ra ngoài Infrastructure.
    /// </summary>
    public IQueryable<T> Query()
        => _dbSet.AsQueryable();

    // ── Commands ──────────────────────────────────────────────────────────

    public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
        => await _dbSet.AddAsync(entity, cancellationToken);

    public async Task AddRangeAsync(
        IEnumerable<T> entities,
        CancellationToken cancellationToken = default)
        => await _dbSet.AddRangeAsync(entities, cancellationToken);

    public Task UpdateAsync(T entity)
    {
        _dbSet.Attach(entity);
        _context.Entry(entity).State = EntityState.Modified;
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await _dbSet.FindAsync(id);
        if (entity is null) return;

        // AppDbContext.SaveChangesAsync tự convert sang soft-delete
        // nếu entity là AuditableEntity, không cần xử lý ở đây.
        _dbSet.Remove(entity);
    }

    // ── Dapper ────────────────────────────────────────────────────────────

    /// <summary>
    /// Mở kết nối Dapper cho raw SQL phức tạp, report, aggregation.
    /// Dùng `using var conn = CreateConnection()` và gọi Query/Execute của Dapper.
    /// </summary>
    protected IDbConnection CreateConnection()
        => new NpgsqlConnection(_context.Database.GetConnectionString());
}
