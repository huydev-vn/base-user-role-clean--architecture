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

/// <summary>
/// User management endpoints.
/// - /me/*  : bất kỳ user đã đăng nhập (tự thao tác trên account của mình).
/// - /{id}/* : Admin/Moderator thao tác trên user bất kỳ.
/// </summary>
[Authorize]
public sealed class UsersController(ISender sender) : BaseController
{
    // ── "Me" Endpoints ────────────────────────────────────────────────────

    /// <summary>
    /// Lấy thông tin profile của user đang đăng nhập.
    /// </summary>
    /// <response code="200">Profile data.</response>
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserDto), 200)]
    public async Task<IActionResult> GetMe(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetCurrentUserQuery(), cancellationToken);
        return OkOrError(result);
    }

    /// <summary>
    /// Cập nhật profile (họ tên, số điện thoại, avatar).
    /// </summary>
    /// <response code="204">Cập nhật thành công.</response>
    /// <response code="400">Validation thất bại.</response>
    [HttpPut("me/profile")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> UpdateMyProfile(
        [FromBody] UpdateProfileRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var result = await sender.Send(
            new UpdateUserProfileCommand(
                userId,
                request.FirstName,
                request.LastName,
                request.PhoneNumber,
                request.AvatarUrl),
            cancellationToken);

        return ToActionResult(result);
    }

    /// <summary>
    /// Đổi mật khẩu — cần cung cấp mật khẩu cũ để xác nhận.
    /// </summary>
    /// <response code="204">Đổi mật khẩu thành công.</response>
    /// <response code="400">Mật khẩu cũ sai hoặc validation thất bại.</response>
    [HttpPut("me/password")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> ChangeMyPassword(
        [FromBody] ChangePasswordRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var result = await sender.Send(
            new ChangePasswordCommand(
                userId,
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
    /// <param name="cancellationToken">Token hủy request khi client ngắt kết nối.</param>
    /// <param name="page">Trang hiện tại (mặc định 1).</param>
    /// <param name="pageSize">Số item mỗi trang, tối đa 100 (mặc định 20).</param>
    /// <param name="search">Tìm theo username, email, họ tên.</param>
    /// <param name="role">Lọc theo role.</param>
    /// <param name="status">Lọc theo trạng thái.</param>
    /// <response code="200">Danh sách users kèm pagination metadata.</response>
    [HttpGet]
    [Authorize(Roles = "Admin,Moderator")]
    [ProducesResponseType(typeof(Application.Common.PagedResult<UserDto>), 200)]
    public async Task<IActionResult> GetAll(
        CancellationToken cancellationToken,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] UserRole? role = null,
        [FromQuery] UserStatus? status = null)
    {
        // Giới hạn pageSize để tránh query quá lớn
        pageSize = Math.Clamp(pageSize, 1, 100);

        var result = await sender.Send(
            new GetAllUsersQuery(page, pageSize, search, role, status),
            cancellationToken);

        return OkOrError(result);
    }

    /// <summary>
    /// Lấy thông tin user theo ID.
    /// </summary>
    /// <response code="200">User data.</response>
    /// <response code="404">User không tồn tại.</response>
    [HttpGet("{id:guid}", Name = "GetUserById")]
    [Authorize(Roles = "Admin,Moderator")]
    [ProducesResponseType(typeof(UserDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetUserByIdQuery(id), cancellationToken);
        return OkOrError(result);
    }

    /// <summary>
    /// Cập nhật profile của user bất kỳ (Admin/Moderator).
    /// </summary>
    /// <response code="204">Cập nhật thành công.</response>
    /// <response code="404">User không tồn tại.</response>
    [HttpPut("{id:guid}/profile")]
    [Authorize(Roles = "Admin,Moderator")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
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

    /// <summary>
    /// Đổi role của user (Admin only).
    /// </summary>
    /// <response code="204">Đổi role thành công.</response>
    /// <response code="404">User không tồn tại.</response>
    [HttpPut("{id:guid}/role")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
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

    /// <summary>
    /// Xóa user (soft-delete — Admin only).
    /// </summary>
    /// <response code="204">Xóa thành công.</response>
    /// <response code="404">User không tồn tại.</response>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteUserCommand(id), cancellationToken);
        return ToActionResult(result);
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private Guid GetCurrentUserId()
        => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // ── User Permission Overrides ─────────────────────────────────────────

    /// <summary>
    /// Lấy danh sách permission overrides của một user cụ thể.
    /// </summary>
    /// <response code="200">Danh sách UserPermission overrides.</response>
    /// <response code="404">User không tồn tại.</response>
    [HttpGet("{id:guid}/permissions")]
    [Authorize(Policy = Permissions.Users.ManagePermissions)]
    [ProducesResponseType(typeof(IReadOnlyList<UserPermissionDto>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetUserPermissions(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetUserPermissionsQuery(id), cancellationToken);
        return OkOrError(result);
    }

    /// <summary>
    /// Grant (cho phép) một permission cho user — tạo override IsGranted = true.
    /// </summary>
    /// <response code="204">Grant thành công.</response>
    /// <response code="400">Permission không tồn tại hoặc override đã tồn tại.</response>
    /// <response code="404">User hoặc Permission không tìm thấy.</response>
    [HttpPost("{id:guid}/permissions/grant")]
    [Authorize(Policy = Permissions.Users.ManagePermissions)]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
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

    /// <summary>
    /// Deny (từ chối) một permission cho user — tạo override IsGranted = false.
    /// </summary>
    /// <response code="204">Deny thành công.</response>
    /// <response code="400">Permission không tồn tại hoặc override đã tồn tại.</response>
    /// <response code="404">User hoặc Permission không tìm thấy.</response>
    [HttpPost("{id:guid}/permissions/deny")]
    [Authorize(Policy = Permissions.Users.ManagePermissions)]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
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

    /// <summary>
    /// Cập nhật IsGranted của một UserPermission override đã tồn tại.
    /// </summary>
    /// <param name="id">User ID.</param>
    /// <param name="permissionId">Permission ID.</param>
    /// <param name="request">Giá trị IsGranted mới.</param>
    /// <param name="cancellationToken">Token hủy request khi client ngắt kết nối.</param>
    /// <response code="204">Cập nhật thành công.</response>
    /// <response code="404">UserPermission override không tồn tại.</response>
    [HttpPut("{id:guid}/permissions/{permissionId:guid}")]
    [Authorize(Policy = Permissions.Users.ManagePermissions)]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
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
}
