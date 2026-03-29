namespace Application.DTOs.Roles;

/// <summary>Request body để gán một Permission cho một Role.</summary>
public sealed record AssignPermissionToRoleRequest(Guid PermissionId);
