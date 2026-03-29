using Application.DTOs.Permissions;
using Application.Features.Permissions.GetAll;
using Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

/// <summary>
/// Quản lý danh mục Permission trong hệ thống.
/// Tất cả endpoints yêu cầu đã đăng nhập và có permission tương ứng.
/// </summary>
[Authorize]
public sealed class PermissionsController(ISender sender) : BaseController
{
    /// <summary>
    /// Lấy danh sách tất cả permissions. Có thể lọc chỉ lấy các permission đang active.
    /// </summary>
    /// <param name="cancellationToken">Token hủy request khi client ngắt kết nối.</param>
    /// <param name="activeOnly">Nếu true, chỉ trả về permissions có IsActive = true (mặc định false).</param>
    /// <response code="200">Danh sách permissions.</response>
    [HttpGet]
    [Authorize(Policy = Permissions.PermissionManagement.Read)]
    [ProducesResponseType(typeof(IReadOnlyList<PermissionDto>), 200)]
    public async Task<IActionResult> GetAll(
        CancellationToken cancellationToken,
        [FromQuery] bool activeOnly = false)
    {
        var result = await sender.Send(new GetAllPermissionsQuery(activeOnly), cancellationToken);
        return OkOrError(result);
    }
}
