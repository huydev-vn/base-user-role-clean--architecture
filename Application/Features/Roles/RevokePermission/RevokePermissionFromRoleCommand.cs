using Application.Common;
using Application.Common.Messaging;
using Domain.Enums;

namespace Application.Features.Roles.RevokePermission;

/// <summary>Thu hồi một permission khỏi một role.</summary>
public sealed record RevokePermissionFromRoleCommand(
    UserRole Role,
    Guid PermissionId
) : ICommand;
