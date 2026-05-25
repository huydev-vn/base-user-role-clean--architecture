using System.Security.Claims;
using Application.DTOs.Permissions;
using Application.DTOs.Users;
using Application.Features.Users.ChangePassword;
using Application.Features.Users.ChangeRole;
using Application.Features.Users.Delete;
using Application.Features.Users.GetAll;
using Application.Features.Users.GetById;
using Application.Features.Users.GetCurrentUser;
using Application.Features.Users.GrantPermission;
using Application.Features.Users.DenyPermission;
using Application.Features.Users.GetPermissions;
using Application.Features.Users.UpdatePermission;
using Application.Features.Users.UpdateProfile;
using Domain.Constants;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize]
public sealed class UsersController(ISender sender) : BaseController
{
    // ── "Me" Endpoints ────────────────────────────────────────────────────

    /// <summary>Lấy thông tin profile của user đang đăng nhập.</summary>
    /// <response code="200">Profile data.</response>
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserDto), 200)]
    public async Task<IActionResult> GetMe(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetCurrentUserQuery(), cancellationToken);
        return OkOrError(result);
    }

    /// <summary>Cập nhật profile (họ tên, số điện thoại, avatar).</summary>
    /// <response code="204">Cập nhật thành công.</response>
    /// <response code="400">Validation thất bại.</response>
    [HttpPut("me/profile")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    public async Task<IActionResult> UpdateMyProfile(
        [FromBody] UpdateProfileRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new UpdateUserProfileCommand(
                GetCurrentUserId(),
                request.FirstName,
                request.LastName,
                request.PhoneNumber,
                request.AvatarUrl),
            cancellationToken);

        return ToActionResult(result);
    }

    /// <summary>Đổi mật khẩu — cần cung cấp mật khẩu cũ.</summary>
    /// <response code="204">Đổi mật khẩu thành công.</response>
    /// <response code="400">Mật khẩu cũ sai hoặc validation thất bại.</response>
    [HttpPut("me/password")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    public async Task<IActionResult> ChangeMyPassword(
        [FromBody] ChangePasswordRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new ChangePasswordCommand(
                GetCurrentUserId(),
                request.CurrentPassword,
                request.NewPassword,
                request.ConfirmNewPassword),
            cancellationToken);

        return ToActionResult(result);
    }

    // ── Admin / Moderator Endpoints ────────────────────────────────────────

    /// <summary>
    /// Lấy danh sách users có phân trang, tìm kiếm và lọc theo role/status.
    /// </summary>
    /// <response code="200">Danh sách users kèm pagination metadata.</response>
    [HttpGet]
    [Authorize(Policy = Permissions.Users.Read)]
    [ProducesResponseType(typeof(Application.Common.PagedResult<UserDto>), 200)]
    public async Task<IActionResult> GetAll(
        CancellationToken cancellationToken,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] UserRole? role = null,
        [FromQuery] UserStatus? status = null)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);

        var result = await sender.Send(
            new GetAllUsersQuery(page, pageSize, search, role, status),
            cancellationToken);

        return OkOrError(result);
    }

    /// <summary>Lấy thông tin user theo ID.</summary>
    /// <response code="200">User data.</response>
    /// <response code="404">User không tồn tại.</response>
    [HttpGet("{id:guid}", Name = "GetUserById")]
    [Authorize(Policy = Permissions.Users.Read)]
    [ProducesResponseType(typeof(UserDto), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetUserByIdQuery(id), cancellationToken);
        return OkOrError(result);
    }

    /// <summary>Cập nhật profile của user bất kỳ.</summary>
    /// <response code="204">Cập nhật thành công.</response>
    /// <response code="404">User không tồn tại.</response>
    [HttpPut("{id:guid}/profile")]
    [Authorize(Policy = Permissions.Users.Update)]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> UpdateProfile(
        Guid id,
        [FromBody] UpdateProfileRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new UpdateUserProfileCommand(
                id,
                request.FirstName,
                request.LastName,
                request.PhoneNumber,
                request.AvatarUrl),
            cancellationToken);

        return ToActionResult(result);
    }

    /// <summary>Đổi role của user (Admin only).</summary>
    /// <response code="204">Đổi role thành công.</response>
    /// <response code="404">User không tồn tại.</response>
    [HttpPut("{id:guid}/role")]
    [Authorize(Policy = Permissions.Users.ChangeRole)]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> ChangeRole(
        Guid id,
        [FromBody] ChangeRoleRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new ChangeUserRoleCommand(id, request.NewRole),
            cancellationToken);

        return ToActionResult(result);
    }

    /// <summary>Xóa user (soft-delete).</summary>
    /// <response code="204">Xóa thành công.</response>
    /// <response code="404">User không tồn tại.</response>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Permissions.Users.Delete)]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteUserCommand(id), cancellationToken);
        return ToActionResult(result);
    }

    // ── User Permission Overrides ─────────────────────────────────────────

    /// <summary>Lấy danh sách permission overrides của một user.</summary>
    /// <response code="200">Danh sách UserPermission overrides.</response>
    /// <response code="404">User không tồn tại.</response>
    [HttpGet("{id:guid}/permissions")]
    [Authorize(Policy = Permissions.Users.ManagePermissions)]
    [ProducesResponseType(typeof(IReadOnlyList<UserPermissionDto>), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> GetUserPermissions(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetUserPermissionsQuery(id), cancellationToken);
        return OkOrError(result);
    }

    /// <summary>Grant một permission cho user (override IsGranted = true).</summary>
    /// <response code="204">Grant thành công.</response>
    /// <response code="400">Override đã tồn tại.</response>
    /// <response code="404">User hoặc Permission không tìm thấy.</response>
    [HttpPost("{id:guid}/permissions/grant")]
    [Authorize(Policy = Permissions.Users.ManagePermissions)]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> GrantPermission(
        Guid id,
        [FromBody] UserPermissionRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GrantUserPermissionCommand(id, request.PermissionId),
            cancellationToken);

        return ToActionResult(result);
    }

    /// <summary>Deny một permission cho user (override IsGranted = false).</summary>
    /// <response code="204">Deny thành công.</response>
    /// <response code="400">Override đã tồn tại.</response>
    /// <response code="404">User hoặc Permission không tìm thấy.</response>
    [HttpPost("{id:guid}/permissions/deny")]
    [Authorize(Policy = Permissions.Users.ManagePermissions)]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> DenyPermission(
        Guid id,
        [FromBody] UserPermissionRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new DenyUserPermissionCommand(id, request.PermissionId),
            cancellationToken);

        return ToActionResult(result);
    }

    /// <summary>Cập nhật IsGranted của một UserPermission override đã tồn tại.</summary>
    /// <response code="204">Cập nhật thành công.</response>
    /// <response code="404">UserPermission override không tồn tại.</response>
    [HttpPut("{id:guid}/permissions/{permissionId:guid}")]
    [Authorize(Policy = Permissions.Users.ManagePermissions)]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> UpdatePermission(
        Guid id,
        Guid permissionId,
        [FromBody] UpdateUserPermissionRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new UpdateUserPermissionCommand(id, permissionId, request.IsGranted),
            cancellationToken);

        return ToActionResult(result);
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private Guid GetCurrentUserId()
        => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
