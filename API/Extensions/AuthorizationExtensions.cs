using Domain.Constants;
using Microsoft.AspNetCore.Authorization;

namespace API.Extensions;

/// <summary>
/// Extension để đăng ký toàn bộ Authorization Policies cho hệ thống.
/// Mỗi policy name = permission string từ <see cref="Permissions"/> — Single Source of Truth.
/// </summary>
public static class AuthorizationExtensions
{
    public static IServiceCollection AddPermissionAuthorization(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            // ── Users ──────────────────────────────────────────────────────
            options.AddPermissionPolicy(Permissions.Users.Read);
            options.AddPermissionPolicy(Permissions.Users.ReadSelf);
            options.AddPermissionPolicy(Permissions.Users.Create);
            options.AddPermissionPolicy(Permissions.Users.Update);
            options.AddPermissionPolicy(Permissions.Users.Delete);
            options.AddPermissionPolicy(Permissions.Users.ChangeRole);
            options.AddPermissionPolicy(Permissions.Users.ManageLockout);
            options.AddPermissionPolicy(Permissions.Users.ManagePermissions);

            // ── Roles ──────────────────────────────────────────────────────
            options.AddPermissionPolicy(Permissions.Roles.Read);
            options.AddPermissionPolicy(Permissions.Roles.ManagePermissions);

            // ── Permission Management ──────────────────────────────────────
            options.AddPermissionPolicy(Permissions.PermissionManagement.Read);
            options.AddPermissionPolicy(Permissions.PermissionManagement.Create);
            options.AddPermissionPolicy(Permissions.PermissionManagement.Update);
            options.AddPermissionPolicy(Permissions.PermissionManagement.ToggleActive);

            // ── Reports ────────────────────────────────────────────────────
            options.AddPermissionPolicy(Permissions.Reports.View);
            options.AddPermissionPolicy(Permissions.Reports.Export);

            // ── Settings ───────────────────────────────────────────────────
            options.AddPermissionPolicy(Permissions.Settings.Read);
            options.AddPermissionPolicy(Permissions.Settings.Update);
        });

        return services;
    }

    /// <summary>
    /// Thêm policy yêu cầu claim "permissions" có value = <paramref name="permission"/>.
    /// Policy name = permission string để dùng trực tiếp: [Authorize(Policy = Permissions.Users.Read)]
    /// </summary>
    private static void AddPermissionPolicy(this AuthorizationOptions options, string permission)
        => options.AddPolicy(permission, policy => policy.RequireClaim("permissions", permission));
}
