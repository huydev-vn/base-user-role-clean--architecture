using Dapper;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public sealed class UserRepository(AppDbContext context)
    : GenericRepository<User>(context), IUserRepository
{
    // ── EF Core queries ───────────────────────────────────────────────────

    public async Task<User?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
        => await _dbSet
            .FirstOrDefaultAsync(
                u => u.Email == email.ToLowerInvariant().Trim(),
                cancellationToken);

    public async Task<User?> GetByUsernameAsync(
        string username,
        CancellationToken cancellationToken = default)
        => await _dbSet
            .FirstOrDefaultAsync(
                u => u.Username == username.ToLowerInvariant().Trim(),
                cancellationToken);

    public async Task<bool> IsUsernameTakenAsync(
        string username,
        CancellationToken cancellationToken = default)
        => await _dbSet
            .AnyAsync(
                u => u.Username == username.ToLowerInvariant().Trim(),
                cancellationToken);

    public async Task<User?> GetByRefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
        => await _dbSet
            .FirstOrDefaultAsync(
                u => u.RefreshToken == refreshToken,
                cancellationToken);

    public async Task<bool> IsEmailTakenAsync(
        string email,
        CancellationToken cancellationToken = default)
        => await _dbSet
            .AnyAsync(
                u => u.Email == email.ToLowerInvariant().Trim(),
                cancellationToken);

    public async Task<IEnumerable<User>> GetByRoleAsync(
        UserRole role,
        CancellationToken cancellationToken = default)
        => await _dbSet
            .Where(u => u.Role == role)
            .ToListAsync(cancellationToken);

    public async Task<(IReadOnlyList<User> Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        string? search = null,
        UserRole? role = null,
        UserStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLowerInvariant();
            query = query.Where(u =>
                u.Username.Contains(s) ||
                u.Email.Contains(s) ||
                u.FirstName.ToLower().Contains(s) ||
                u.LastName.ToLower().Contains(s));
        }

        if (role.HasValue)
            query = query.Where(u => u.Role == role.Value);

        if (status.HasValue)
            query = query.Where(u => u.Status == status.Value);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    // ── Dapper query (ví dụ: report thống kê user theo role) ─────────────

    public async Task<IEnumerable<(UserRole Role, int Count)>> GetUserCountByRoleAsync()
    {
        using var conn = CreateConnection();
        const string sql = """
            SELECT "Role", COUNT(*) AS "Count"
            FROM "Users"
            WHERE "IsDeleted" = false
            GROUP BY "Role"
            ORDER BY "Count" DESC
            """;

        var result = await conn.QueryAsync<(string Role, int Count)>(sql);
        return result.Select(r => (Enum.Parse<UserRole>(r.Role), r.Count));
    }
}
