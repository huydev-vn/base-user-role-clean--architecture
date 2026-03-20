using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces.Repositories;

namespace Domain.Interfaces.Repositories;

/// <summary>
/// Repository đặc thù cho User — kế thừa toàn bộ CRUD từ IGenericRepository.
/// Chỉ khai báo thêm các query đặc thù của User domain.
/// </summary>
public interface IUserRepository : IGenericRepository<User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);

    Task<User?> GetByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);

    Task<bool> IsEmailTakenAsync(string email, CancellationToken cancellationToken = default);

    Task<bool> IsUsernameTakenAsync(string username, CancellationToken cancellationToken = default);

    Task<IEnumerable<User>> GetByRoleAsync(UserRole role, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<User> Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        string? search = null,
        UserRole? role = null,
        UserStatus? status = null,
        CancellationToken cancellationToken = default);
}
