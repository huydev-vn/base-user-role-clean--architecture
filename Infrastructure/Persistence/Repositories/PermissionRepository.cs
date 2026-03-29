using Domain.Entities;
using Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation cho Permission entity.
/// Cung cấp các query đặc thù ngoài CRUD chung trong GenericRepository.
/// </summary>
public sealed class PermissionRepository(AppDbContext context)
    : GenericRepository<Permission>(context), IPermissionRepository
{
    /// <summary>
    /// Tìm permission theo Name — case-insensitive.
    /// Dùng khi check duplicates hoặc load permission để display.
    /// </summary>
    public async Task<Permission?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return await _dbSet
            .FirstOrDefaultAsync(
                p => p.Name.ToLower() == name.ToLower().Trim(),
                cancellationToken);
    }

    /// <summary>
    /// Lấy tất cả permissions của một group.
    /// Dùng khi hiển thị admin UI theo nhóm chức năng.
    /// </summary>
    public async Task<IReadOnlyList<Permission>> GetByGroupAsync(
        string group,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(group);

        var permissions = await _dbSet
            .Where(p => p.Group.ToLower() == group.ToLower().Trim())
            .ToListAsync(cancellationToken);

        return permissions.AsReadOnly();
    }

    /// <summary>
    /// Lấy tất cả permissions đang IsActive = true.
    /// Dùng khi build policy list lúc khởi động ứng dụng.
    /// </summary>
    public async Task<IReadOnlyList<Permission>> GetAllActiveAsync(
        CancellationToken cancellationToken = default)
    {
        var permissions = await _dbSet
            .Where(p => p.IsActive)
            .ToListAsync(cancellationToken);

        return permissions.AsReadOnly();
    }

    /// <summary>
    /// Lấy danh sách tất cả group names hiện có (distinct).
    /// Dùng cho admin UI — dropdown hoặc multi-select filter.
    /// </summary>
    public async Task<IReadOnlyList<string>> GetAllGroupsAsync(
        CancellationToken cancellationToken = default)
    {
        var groups = await _dbSet
            .Select(p => p.Group)
            .Distinct()
            .OrderBy(g => g)
            .ToListAsync(cancellationToken);

        return groups.AsReadOnly();
    }

    /// <summary>
    /// Kiểm tra Name đã tồn tại chưa — case-insensitive.
    /// Dùng khi tạo mới hoặc cập nhật permission để validate unique constraint.
    /// </summary>
    /// <param name="name">Tên permission, ví dụ "Users.Read".</param>
    /// <param name="excludeId">
    /// Bỏ qua record có Id này.
    /// Dùng khi validate Update: không báo lỗi khi name là của chính entity đang edit.
    /// </param>
    public async Task<bool> IsNameTakenAsync(
        string name,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var query = _dbSet.Where(p => p.Name.ToLower() == name.ToLower().Trim());

        if (excludeId.HasValue)
            query = query.Where(p => p.Id != excludeId.Value);

        return await query.AnyAsync(cancellationToken);
    }
}
