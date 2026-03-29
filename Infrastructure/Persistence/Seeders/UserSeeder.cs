using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Persistence.Seeders;

/// <summary>
/// Seeder tạo các tài khoản mặc định (Admin, Moderator) khi hệ thống khởi chạy lần đầu.
///
/// Cấu hình trong appsettings.json:
/// <code>
/// "DefaultAccounts": {
///   "Admin": {
///     "Username": "admin",
///     "Email": "admin@example.com",
///     "Password": "Admin@123456",
///     "FirstName": "System",
///     "LastName": "Admin"
///   },
///   "Moderator": {
///     "Username": "moderator",
///     "Email": "moderator@example.com",
///     "Password": "Mod@123456",
///     "FirstName": "System",
///     "LastName": "Moderator"
///   }
/// }
/// </code>
///
/// Idempotent: bỏ qua nếu username/email đã tồn tại.
/// </summary>
public sealed class UserSeeder(
    AppDbContext context,
    IPasswordHasher passwordHasher,
    IConfiguration configuration,
    ILogger<UserSeeder> logger) : IUserSeeder
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await SeedAccountAsync("Admin", UserRole.Admin, cancellationToken);
        await SeedAccountAsync("Moderator", UserRole.Moderator, cancellationToken);

        if (context.ChangeTracker.HasChanges())
            await context.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedAccountAsync(
        string sectionKey,
        UserRole role,
        CancellationToken cancellationToken)
    {
        var section = configuration.GetSection($"DefaultAccounts:{sectionKey}");

        var username  = section["Username"];
        var email     = section["Email"];
        var password  = section["Password"];
        var firstName = section["FirstName"] ?? sectionKey;
        var lastName  = section["LastName"]  ?? "Account";

        if (string.IsNullOrWhiteSpace(username) ||
            string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password))
        {
            logger.LogWarning(
                "DefaultAccounts:{Section} thiếu cấu hình — bỏ qua seed tài khoản {Role}.",
                sectionKey, role);
            return;
        }

        // Idempotent: bỏ qua nếu tài khoản đã tồn tại
        var alreadyExists = await context.Users
            .AsNoTracking()
            .AnyAsync(u => u.Username == username.ToLowerInvariant() ||
                           u.Email    == email.ToLowerInvariant(),
                      cancellationToken);

        if (alreadyExists)
        {
            logger.LogDebug(
                "Tài khoản mặc định '{Username}' ({Role}) đã tồn tại — bỏ qua.",
                username, role);
            return;
        }

        var passwordHash = passwordHasher.Hash(password);

        // Tài khoản hệ thống: email đã được xác thực, không cần verification token
        var user = User.Create(
            username:                username,
            firstName:               firstName,
            lastName:                lastName,
            email:                   email,
            passwordHash:            passwordHash,
            emailVerificationToken:  Guid.NewGuid().ToString("N"),
            role:                    role);

        // Activate ngay — tài khoản hệ thống không cần xác thực email
        user.VerifyEmail();

        context.Users.Add(user);

        logger.LogInformation(
            "Đã tạo tài khoản mặc định '{Username}' với role {Role}.",
            username, role);
    }
}
