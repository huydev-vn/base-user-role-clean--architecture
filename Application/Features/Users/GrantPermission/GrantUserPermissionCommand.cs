using Application.Common;
using Application.Common.Messaging;

namespace Application.Features.Users.GrantPermission;

/// <summary>Cấp thêm permission cho user vượt ngoài role của họ (explicit grant).</summary>
public sealed record GrantUserPermissionCommand(
    Guid UserId,
    Guid PermissionId
) : ICommand;
