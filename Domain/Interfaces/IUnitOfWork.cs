namespace Domain.Interfaces;

/// <summary>
/// Unit of Work — đảm bảo nhiều thao tác repository chạy trong CÙNG một transaction.
/// Nếu không có UoW: repo A save thành công, repo B lỗi → data không nhất quán.
/// Với UoW: cả hai thành công mới commit, một bên lỗi thì rollback hết.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Lưu tất cả thay đổi đang pending vào DB trong một transaction.
    /// Sau khi SaveChanges, Domain Events sẽ được dispatch.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Bắt đầu explicit transaction — dùng khi cần kiểm soát thủ công.
    /// Thông thường chỉ cần gọi SaveChangesAsync() là đủ.
    /// </summary>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
