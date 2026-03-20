namespace Domain.Enums;

public enum UserStatus
{
    /// <summary>Vừa đăng ký, chưa xác thực email.</summary>
    PendingVerification = 0,

    /// <summary>Đã xác thực, hoạt động bình thường.</summary>
    Active = 1,

    /// <summary>Tự tắt tài khoản hoặc admin vô hiệu hóa.</summary>
    Inactive = 2,

    /// <summary>Bị khóa do vi phạm.</summary>
    Banned = 3
}
