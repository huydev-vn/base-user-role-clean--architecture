using Domain.Common;
using Domain.Enums;
using Domain.Events.Users;
using Domain.ValueObjects;

namespace Domain.Entities;

/// <summary>
/// User entity — trung tâm của authentication/authorization.
///
/// Nguyên tắc thiết kế:
/// - Properties có setter private: chỉ thay đổi qua domain methods, không set tay từ ngoài.
/// - Factory method Create(): điểm duy nhất tạo User, đảm bảo invariants.
/// - Mỗi thay đổi trạng thái quan trọng đều raise DomainEvent.
/// - Không chứa business logic của tầng khác (validation, hashing ở Application/Infrastructure).
/// </summary>
public sealed class User : AuditableEntity
{
    // ── Identity ──────────────────────────────────────────────────────────
    public string Username { get; private set; } = string.Empty;
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;

    // ── Profile ───────────────────────────────────────────────────────────
    public string? PhoneNumber { get; private set; }
    public string? AvatarUrl { get; private set; }

    // ── Auth ──────────────────────────────────────────────────────────────
    public UserRole Role { get; private set; }
    public UserStatus Status { get; private set; }

    /// <summary>Token dùng để cấp lại Access Token khi hết hạn.</summary>
    public string? RefreshToken { get; private set; }
    public DateTime? RefreshTokenExpiresAt { get; private set; }

    /// <summary>Số lần đăng nhập sai liên tiếp — dùng để lockout.</summary>
    public int FailedLoginAttempts { get; private set; }
    public DateTime? LockedUntil { get; private set; }

    /// <summary>Token gửi qua email để xác thực tài khoản.</summary>
    public string? EmailVerificationToken { get; private set; }
    public bool IsEmailVerified { get; private set; }

    // ── Permission Overrides ──────────────────────────────────────────────

    private readonly List<UserPermission> _userPermissions = [];

    /// <summary>
    /// User-specific permission overrides — cấp thêm hoặc thu hồi quyền so với Role.
    /// Xem UserPermission để hiểu thuật toán effective permissions.
    /// </summary>
    public IReadOnlyCollection<UserPermission> UserPermissions => _userPermissions.AsReadOnly();

    // ── Computed ──────────────────────────────────────────────────────────
    public string FullName => $"{FirstName} {LastName}".Trim();
    public bool IsLocked => LockedUntil.HasValue && LockedUntil > DateTime.UtcNow;
    public bool IsActive => Status == UserStatus.Active;

    // EF Core cần constructor không tham số
    private User() { }

    // ── Factory Method ────────────────────────────────────────────────────

    /// <summary>
    /// Điểm duy nhất tạo User mới.
    /// Application layer gọi sau khi đã hash password và tạo verification token.
    /// </summary>
    public static User Create(
        string username,
        string firstName,
        string lastName,
        string email,
        string passwordHash,
        string emailVerificationToken,
        UserRole role = UserRole.User)
    {
        // Validate email format qua Value Object
        var emailVo = ValueObjects.Email.Create(email);

        var user = new User
        {
            Username = username.ToLowerInvariant().Trim(),
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            Email = emailVo.Value,
            PasswordHash = passwordHash,
            Role = role,
            Status = UserStatus.PendingVerification,
            EmailVerificationToken = emailVerificationToken,
            IsEmailVerified = false,
            FailedLoginAttempts = 0
        };

        user.RaiseDomainEvent(new UserCreatedEvent(user.Id, user.Email, user.FullName));

        return user;
    }

    // ── Domain Methods ────────────────────────────────────────────────────

    public void VerifyEmail()
    {
        if (IsEmailVerified) return;

        IsEmailVerified = true;
        EmailVerificationToken = null;
        Status = UserStatus.Active;

        RaiseDomainEvent(new UserActivatedEvent(Id, Email));
    }

    public void UpdateProfile(string firstName, string lastName, string? phoneNumber, string? avatarUrl)
    {
        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        PhoneNumber = phoneNumber?.Trim();
        AvatarUrl = avatarUrl?.Trim();

        RaiseDomainEvent(new UserUpdatedEvent(Id, Email, FullName));
    }

    public void ChangePassword(string newPasswordHash)
    {
        PasswordHash = newPasswordHash;
        RefreshToken = null;
        RefreshTokenExpiresAt = null;
    }

    public void SetRefreshToken(string token, DateTime expiresAt)
    {
        RefreshToken = token;
        RefreshTokenExpiresAt = expiresAt;
    }

    public void RevokeRefreshToken()
    {
        RefreshToken = null;
        RefreshTokenExpiresAt = null;
    }

    public void RecordFailedLogin(int maxAttempts = 5, int lockoutMinutes = 15)
    {
        FailedLoginAttempts++;

        if (FailedLoginAttempts >= maxAttempts)
        {
            LockedUntil = DateTime.UtcNow.AddMinutes(lockoutMinutes);
            RaiseDomainEvent(new UserLockedOutEvent(Id, Email, LockedUntil.Value));
        }
    }

    public void RecordSuccessfulLogin()
    {
        FailedLoginAttempts = 0;
        LockedUntil = null;
    }

    public void ChangeRole(UserRole newRole)
    {
        Role = newRole;
    }

    public void Deactivate()
    {
        Status = UserStatus.Inactive;
    }

    public void Ban()
    {
        Status = UserStatus.Banned;
        RefreshToken = null;
        RefreshTokenExpiresAt = null;
    }
}
