namespace Domain.Constants;

/// <summary>
/// Catalog tất cả permissions trong hệ thống — Single Source of Truth.
///
/// Mục đích:
///   1. Type-safe policy names: [Authorize(Policy = Permissions.Users.Delete)]
///   2. Seeding: Infrastructure dùng để tạo Permission entities vào DB lần đầu.
///   3. Tránh magic strings và typos.
///
/// Quy tắc:
///   - Mỗi const string PHẢI match chính xác với Permission.Name trong DB.
///   - Convention: "{Group}.{Action}" — cả hai phần đều viết hoa chữ cái đầu.
///   - Khi thêm permission mới: thêm const ở đây TRƯỚC, sau đó tạo seed data.
///   - Không xóa const đã có — chỉ deprecate bằng [Obsolete] nếu cần.
/// </summary>
public static class Permissions
{
    // ── Users ─────────────────────────────────────────────────────────────

    /// <summary>Quyền liên quan đến quản lý người dùng.</summary>
    public static class Users
    {
        /// <summary>Xem danh sách và chi tiết bất kỳ user nào.</summary>
        public const string Read = "Users.Read";

        /// <summary>Xem thông tin profile của chính mình. Tất cả user đã đăng nhập đều có.</summary>
        public const string ReadSelf = "Users.ReadSelf";

        /// <summary>Tạo mới user (admin tạo thủ công, khác với self-register).</summary>
        public const string Create = "Users.Create";

        /// <summary>Cập nhật thông tin profile của bất kỳ user nào.</summary>
        public const string Update = "Users.Update";

        /// <summary>Xóa (soft delete) user khỏi hệ thống.</summary>
        public const string Delete = "Users.Delete";

        /// <summary>Thay đổi Role của user (ví dụ: nâng User → Moderator).</summary>
        public const string ChangeRole = "Users.ChangeRole";

        /// <summary>Khóa / mở khóa tài khoản user.</summary>
        public const string ManageLockout = "Users.ManageLockout";

        /// <summary>Cấp / thu hồi UserPermission override cho user cụ thể.</summary>
        public const string ManagePermissions = "Users.ManagePermissions";
    }

    // ── Roles ─────────────────────────────────────────────────────────────

    /// <summary>Quyền liên quan đến quản lý role và phân quyền.</summary>
    public static class Roles
    {
        /// <summary>Xem danh sách roles và permissions được gán.</summary>
        public const string Read = "Roles.Read";

        /// <summary>Gán / thu hồi Permission cho Role.</summary>
        public const string ManagePermissions = "Roles.ManagePermissions";
    }

    // ── Permissions ───────────────────────────────────────────────────────

    /// <summary>Quyền quản lý danh mục Permission.</summary>
    public static class PermissionManagement
    {
        /// <summary>Xem danh sách tất cả permissions trong hệ thống.</summary>
        public const string Read = "Permissions.Read";

        /// <summary>Tạo permission mới trong hệ thống.</summary>
        public const string Create = "Permissions.Create";

        /// <summary>Cập nhật displayName và description của permission.</summary>
        public const string Update = "Permissions.Update";

        /// <summary>Activate / Deactivate permission.</summary>
        public const string ToggleActive = "Permissions.ToggleActive";
    }

    // ── Thêm group mới ở đây khi mở rộng ─────────────────────────────────
    // public static class Reports
    // {
    //     public const string View   = "Reports.View";
    //     public const string Export = "Reports.Export";
    // }
    //
    // public static class Settings
    // {
    //     public const string Read   = "Settings.Read";
    //     public const string Update = "Settings.Update";
    // }
}
