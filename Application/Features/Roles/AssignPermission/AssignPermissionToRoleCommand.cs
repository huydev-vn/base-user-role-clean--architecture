using Application.Common;
using Application.Common.Messaging;
using Domain.Enums;

namespace Application.Features.Roles.AssignPermission;

/// <summary>Gán một permission cho một role.</summary>
public sealed record AssignPermissionToRoleCommand(
    UserRole Role,
    Guid PermissionId
) : ICommand;
