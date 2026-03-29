using Domain.Constants;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Seeders;

/// <summary>
/// Seeder cho Permission entities.
/// Lấy dữ liệu từ Domain.Constants.Permissions (Single Source of Truth).
///
/// Mục đích:
///   - Tạo tất cả permissions defined trong Permissions catalog vào DB.
///   - Chạy một lần lúc migrations (hoặc DB initialization).
///   - Idempotent: gọi nhiều lần không tạo duplicate.
///   - Khi thêm permission mới: thêm const vào Permissions.cs, seeder sẽ tự pick up.
///
/// Cách sử dụng:
///   - Inject IPermissionSeeder vào handler/startup, gọi SeedAsync().
///   - Hoặc chạy sau khi Database.MigrateAsync() trong Program.cs.
/// </summary>
public sealed class PermissionSeeder(AppDbContext context) : IPermissionSeeder
{
    /// <summary>
    /// Seed tất cả permissions vào database.
    /// Idempotent: chỉ tạo nếu Name chưa tồn tại.
    /// </summary>
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var permissions = GetPermissions();

        foreach (var permission in permissions)
        {
            // Kiểm tra permission đã tồn tại chưa
            var exists = await context.Permissions
                .AsNoTracking()
                .AnyAsync(p => p.Name == permission.Name, cancellationToken);

            if (!exists)
            {
                context.Permissions.Add(permission);
            }
        }

        // Chỉ save nếu có changes
        if (context.ChangeTracker.HasChanges())
        {
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    /// <summary>
    /// Tạo catalog tất cả default permissions từ Domain.Constants.Permissions.
    /// Thêm permission mới: chỉ cần thêm const vào Permissions.cs, đây sẽ tự update.
    ///
    /// Convention:
    ///   - Name từ const value: "Users.Read", "Reports.Export"
    ///   - DisplayName: friendly description cho admin UI
    ///   - Description: chi tiết permissions cho admin (optional, có thể để null)
    /// </summary>
    private static List<Permission> GetPermissions()
    {
        return new List<Permission>
        {
            // ── Users ───────────────────────────────────────────────────────
            Permission.Create(
                Permissions.Users.Read,
                "Xem danh sách và chi tiết bất kỳ người dùng nào",
                "Cho phép xem tất cả thông tin user trong hệ thống."),

            Permission.Create(
                Permissions.Users.ReadSelf,
                "Xem thông tin profile của chính mình",
                "Tất cả user đã đăng nhập đều có quyền này."),

            Permission.Create(
                Permissions.Users.Create,
                "Tạo mới người dùng",
                "Admin tạo user thủ công. Khác với self-register."),

            Permission.Create(
                Permissions.Users.Update,
                "Cập nhật thông tin người dùng",
                "Chỉnh sửa FirstName, LastName, Phone, Avatar của bất kỳ user."),

            Permission.Create(
                Permissions.Users.Delete,
                "Xóa người dùng",
                "Soft delete user khỏi hệ thống. Audit trail vẫn lưu."),

            Permission.Create(
                Permissions.Users.ChangeRole,
                "Thay đổi role người dùng",
                "Nâng hoặc hạ user lên role khác. Ví dụ: User → Moderator."),

            Permission.Create(
                Permissions.Users.ManageLockout,
                "Quản lý khóa tài khoản",
                "Khóa/mở khóa tài khoản user. Dùng khi user bị lock vì login sai nhiều lần."),

            Permission.Create(
                Permissions.Users.ManagePermissions,
                "Quản lý quyền hạn người dùng",
                "Cấp/thu hồi UserPermission overrides cho user cụ thể."),

            // ── Roles ───────────────────────────────────────────────────────
            Permission.Create(
                Permissions.Roles.Read,
                "Xem danh sách roles và permissions",
                "Xem role nào có permissions gì. Dùng trong admin UI."),

            Permission.Create(
                Permissions.Roles.ManagePermissions,
                "Quản lý phân quyền của role",
                "Gán/thu hồi permissions cho/khỏi role. Rất quan trọng nên cần Super Admin."),

            // ── Permissions ─────────────────────────────────────────────────
            Permission.Create(
                Permissions.PermissionManagement.Read,
                "Xem danh sách permissions",
                "Xem tất cả permissions trong hệ thống."),

            Permission.Create(
                Permissions.PermissionManagement.Create,
                "Tạo permission mới",
                "Thêm permission mới vào catalog hệ thống. Cần custom seeding."),

            Permission.Create(
                Permissions.PermissionManagement.Update,
                "Cập nhật permission",
                "Chỉnh DisplayName, Description, nhưng Name là immutable."),

            Permission.Create(
                Permissions.PermissionManagement.ToggleActive,
                "Bật/tắt permission",
                "Deactivate permission sẽ làm tất cả user mất quyền đó ngay lập tức."),
        };
    }
}
