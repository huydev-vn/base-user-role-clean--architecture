using Domain.Enums;

namespace Application.DTOs.Permissions;

/// <summary>Response DTO cho RolePermission — hiển thị permission được gán cho một role.</summary>
public sealed record RolePermissionDto(
    Guid Id,
    UserRole Role,
    Guid PermissionId,
    string PermissionName,
    string PermissionDisplayName,
    string PermissionGroup,
    DateTime CreatedAt
);
