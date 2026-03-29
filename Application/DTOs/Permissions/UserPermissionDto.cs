namespace Application.DTOs.Permissions;

/// <summary>
/// Response DTO cho UserPermission — hiển thị permission override của một user cụ thể.
/// IsGranted = true  → explicit grant (thêm ngoài role).
/// IsGranted = false → explicit deny  (thu hồi dù role có).
/// </summary>
public sealed record UserPermissionDto(
    Guid Id,
    Guid UserId,
    Guid PermissionId,
    string PermissionName,
    string PermissionDisplayName,
    string PermissionGroup,
    bool IsGranted,
    DateTime CreatedAt
);
