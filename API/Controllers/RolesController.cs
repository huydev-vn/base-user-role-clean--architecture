using Application.DTOs.Permissions;
using Application.DTOs.Roles;
using Application.Features.Roles.AssignPermission;
using Application.Features.Roles.GetPermissions;
using Application.Features.Roles.RevokePermission;
using Domain.Constants;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

/// <summary>
/// Quản lý permissions được gán cho các Role.
/// Route: api/roles/{role}/permissions
/// </summary>
[Authorize]
public sealed class RolesController(ISender sender) : BaseController
{
    /// <summary>
    /// Lấy danh sách permissions được gán cho một role cụ thể.
    /// </summary>
    /// <param name="role">Role cần xem (ví dụ: Admin, Moderator, User).</param>
    /// <param name="cancellationToken">Token hủy request khi client ngắt kết nối.</param>
    /// <response code="200">Danh sách role-permission mappings.</response>
    [HttpGet("{role}/permissions")]
    [Authorize(Policy = Permissions.Roles.Read)]
    [ProducesResponseType(typeof(IReadOnlyList<RolePermissionDto>), 200)]
    public async Task<IActionResult> GetPermissions(
        UserRole role,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetRolePermissionsQuery(role), cancellationToken);
        return OkOrError(result);
    }

    /// <summary>
    /// Gán một permission cho role.
    /// </summary>
    /// <param name="role">Role nhận permission.</param>
    /// <param name="request">PermissionId cần gán.</param>
    /// <param name="cancellationToken">Token hủy request khi client ngắt kết nối.</param>
    /// <response code="204">Gán thành công.</response>
    /// <response code="400">Permission không tồn tại, không active, hoặc đã được gán.</response>
    /// <response code="404">Permission không tìm thấy.</response>
    [HttpPost("{role}/permissions")]
    [Authorize(Policy = Permissions.Roles.ManagePermissions)]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> AssignPermission(
        UserRole role,
        [FromBody] AssignPermissionToRoleRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new AssignPermissionToRoleCommand(role, request.PermissionId),
            cancellationToken);

        return ToActionResult(result);
    }

    /// <summary>
    /// Thu hồi một permission khỏi role.
    /// </summary>
    /// <param name="role">Role cần thu hồi permission.</param>
    /// <param name="permissionId">ID của permission cần thu hồi.</param>
    /// <param name="cancellationToken">Token hủy request khi client ngắt kết nối.</param>
    /// <response code="204">Thu hồi thành công.</response>
    /// <response code="404">Role-Permission mapping không tồn tại.</response>
    [HttpDelete("{role}/permissions/{permissionId:guid}")]
    [Authorize(Policy = Permissions.Roles.ManagePermissions)]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> RevokePermission(
        UserRole role,
        Guid permissionId,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new RevokePermissionFromRoleCommand(role, permissionId),
            cancellationToken);

        return ToActionResult(result);
    }
}
