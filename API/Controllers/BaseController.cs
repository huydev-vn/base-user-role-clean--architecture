using Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

/// <summary>
/// Base controller — tất cả controller kế thừa class này.
/// Cung cấp helper methods để trả response chuẩn từ Result pattern.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public abstract class BaseController : ControllerBase
{
    /// <summary>200 OK với value, hoặc 400 Bad Request với error message.</summary>
    protected ActionResult<T> ToActionResult<T>(Result<T> result)
    {
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });
        return Ok(result.Value);
    }

    /// <summary>Trả IActionResult — dùng khi method signature là IActionResult.</summary>
    protected IActionResult OkOrError<T>(Result<T> result)
    {
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });
        return Ok(result.Value);
    }

    /// <summary>204 No Content, hoặc 400 Bad Request với error message.</summary>
    protected ActionResult ToActionResult(Result result)
    {
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });
        return NoContent();
    }

    /// <summary>201 Created với value, hoặc 409 Conflict nếu đã tồn tại.</summary>
    protected ActionResult<T> ToCreatedResult<T>(Result<T> result, string? routeName = null, object? routeValues = null)
    {
        if (!result.IsSuccess)
            return Conflict(new { error = result.Error });

        if (routeName is not null)
            return CreatedAtRoute(routeName, routeValues, result.Value);

        return StatusCode(201, result.Value);
    }

    /// <summary>200 OK với value, hoặc 401 Unauthorized với error message.</summary>
    protected ActionResult<T> ToAuthResult<T>(Result<T> result)
    {
        if (!result.IsSuccess)
            return Unauthorized(new { error = result.Error });
        return Ok(result.Value);
    }
}
