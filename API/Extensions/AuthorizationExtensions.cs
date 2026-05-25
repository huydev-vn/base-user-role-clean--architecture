using System.Reflection;
using Domain.Constants;
using Microsoft.AspNetCore.Authorization;

namespace API.Extensions;

public static class AuthorizationExtensions
{
    public static IServiceCollection AddPermissionAuthorization(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            // Auto-discover tất cả permission constants từ Domain.Constants.Permissions
            // qua reflection — không cần đăng ký tay từng permission khi thêm mới.
            foreach (var permission in GetAllPermissions())
                options.AddPolicy(permission,
                    policy => policy.RequireClaim("permissions", permission));
        });

        return services;
    }

    private static IEnumerable<string> GetAllPermissions()
        => typeof(Permissions)
            .GetNestedTypes(BindingFlags.Public | BindingFlags.Static)
            .SelectMany(t => t.GetFields(BindingFlags.Public | BindingFlags.Static))
            .Where(f => f.IsLiteral && !f.IsInitOnly && f.FieldType == typeof(string))
            .Select(f => (string)f.GetRawConstantValue()!);
}
