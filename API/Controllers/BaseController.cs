using Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseController : ControllerBase
{
    /// <summary>200 OK, hoặc HTTP error tương ứng với ErrorType.</summary>
    protected IActionResult OkOrError<T>(Result<T> result)
    {
        if (result.IsSuccess) return Ok(result.Value);
        return MapError(result.Error!, result.ErrorType);
    }

    /// <summary>204 No Content, hoặc HTTP error tương ứng với ErrorType.</summary>
    protected IActionResult ToActionResult(Result result)
    {
        if (result.IsSuccess) return NoContent();
        return MapError(result.Error!, result.ErrorType);
    }

    /// <summary>201 Created với value, hoặc HTTP error tương ứng với ErrorType.</summary>
    protected IActionResult ToCreatedResult<T>(
        Result<T> result,
        string? routeName = null,
        object? routeValues = null)
    {
        if (!result.IsSuccess) return MapError(result.Error!, result.ErrorType);

        if (routeName is not null)
            return CreatedAtRoute(routeName, routeValues, result.Value);

        return StatusCode(201, result.Value);
    }

    // ── Private ───────────────────────────────────────────────────────────

    private IActionResult MapError(string message, ErrorType errorType)
    {
        var problem = Problem(detail: message, statusCode: MapStatusCode(errorType));
        return StatusCode(problem.StatusCode ?? 400, problem.Value);
    }

    private static int MapStatusCode(ErrorType errorType) => errorType switch
    {
        ErrorType.NotFound     => StatusCodes.Status404NotFound,
        ErrorType.Conflict     => StatusCodes.Status409Conflict,
        ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
        ErrorType.Forbidden    => StatusCodes.Status403Forbidden,
        ErrorType.Unexpected   => StatusCodes.Status500InternalServerError,
        _                      => StatusCodes.Status400BadRequest   // Validation
    };
}
