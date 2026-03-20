using Domain.Enums;

namespace Application.DTOs.Users;

public sealed record ChangeRoleRequest(UserRole NewRole);
