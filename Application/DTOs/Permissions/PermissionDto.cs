namespace Application.DTOs.Permissions;

/// <summary>Response DTO cho Permission entity.</summary>
public sealed record PermissionDto(
    Guid Id,
    string Name,
    string DisplayName,
    string? Description,
    string Group,
    bool IsActive,
    DateTime CreatedAt
);
