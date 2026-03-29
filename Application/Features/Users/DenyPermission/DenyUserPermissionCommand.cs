using Application.Common;
using Application.Common.Messaging;

namespace Application.Features.Users.DenyPermission;

/// <summary>Thu hồi permission của user dù role của họ có quyền đó (explicit deny).</summary>
public sealed record DenyUserPermissionCommand(
    Guid UserId,
    Guid PermissionId
) : ICommand;
