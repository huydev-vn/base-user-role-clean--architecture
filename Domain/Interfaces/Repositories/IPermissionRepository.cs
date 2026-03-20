using Domain.Entities;

namespace Domain.Interfaces.Repositories;

/// <summary>
/// Repository cho Permission entity.
/// Kế thừa toàn bộ CRUD từ IGenericRepository.
/// Bổ sung các query đặc thù của Permission domain.
/// </summary>
public interface IPermissionRepository : IGenericRepository<Permission>
{
    /// <summary>
    /// Tìm permission theo tên (unique). Case-insensitive.
    /// Dùng để check trùng lặp trước khi tạo mới.
    /// </summary>
    Task<Permission?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy tất cả permissions thuộc một group.
    /// Dùng cho admin UI để hiển thị theo nhóm chức năng.
    /// </summary>
    Task<IReadOnlyList<Permission>> GetByGroupAsync(string group, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy tất cả permissions đang IsActive = true.
    /// Dùng khi build policy list lúc khởi động ứng dụng.
    /// </summary>
    Task<IReadOnlyList<Permission>> GetAllActiveAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy danh sách tất cả group names hiện có (distinct).
    /// Dùng cho admin UI để hiển thị filter theo group.
    /// </summary>
    Task<IReadOnlyList<string>> GetAllGroupsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Kiểm tra tên đã tồn tại chưa — dùng khi tạo mới để đảm bảo unique.
    /// </summary>
    /// <param name="name">Tên cần kiểm tra.</param>
    /// <param name="excludeId">
    /// Bỏ qua record có Id này — dùng khi validate Update để không báo lỗi với chính nó.
    /// </param>
    Task<bool> IsNameTakenAsync(
        string name,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default);
}
