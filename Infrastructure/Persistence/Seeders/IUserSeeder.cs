namespace Infrastructure.Persistence.Seeders;

/// <summary>
/// Interface cho user seeding — tạo tài khoản mặc định khi hệ thống khởi chạy lần đầu.
/// </summary>
public interface IUserSeeder
{
    Task SeedAsync(CancellationToken cancellationToken = default);
}
