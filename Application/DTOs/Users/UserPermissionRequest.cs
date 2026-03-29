namespace Application.DTOs.Users;

/// <summary>Request body để grant hoặc deny một Permission cho User.</summary>
public sealed record UserPermissionRequest(Guid PermissionId);
