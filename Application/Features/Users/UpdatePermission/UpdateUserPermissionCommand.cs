using Application.Common;
using Application.Common.Messaging;

namespace Application.Features.Users.UpdatePermission;

/// <summary>
/// Đổi trạng thái override của một UserPermission: Grant ↔ Deny.
/// Dùng khi Admin đổi ý mà không muốn xóa record và tạo lại.
/// </summary>
public sealed record UpdateUserPermissionCommand(
    Guid UserId,
    Guid PermissionId,
    bool IsGranted
) : ICommand;
