namespace Application.DTOs.Users;

/// <summary>Request body để cập nhật trạng thái IsGranted của một UserPermission override.</summary>
public sealed record UpdateUserPermissionRequest(bool IsGranted);
