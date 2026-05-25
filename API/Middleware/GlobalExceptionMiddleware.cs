using System.Text.Json;
using Application.Exceptions;
using Microsoft.AspNetCore.Mvc;
using ValidationException = Application.Exceptions.ValidationException;

namespace API.Middleware;

/// <summary>
/// Bắt toàn bộ exception chưa được xử lý, trả về RFC 7807 ProblemDetails.
/// Không cần try/catch trong bất kỳ Controller nào.
/// </summary>
public sealed class GlobalExceptionMiddleware(
    RequestDelegate next,
    ILogger<GlobalExceptionMiddleware> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception on {Method} {Path}: {Message}",
                context.Request.Method, context.Request.Path, ex.Message);

            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title, extensions) = exception switch
        {
            ValidationException vex => (
                StatusCodes.Status400BadRequest,
                "Dữ liệu không hợp lệ.",
                (object?)new { errors = vex.Errors }),

            NotFoundException nex => (
                StatusCodes.Status404NotFound,
                "Không tìm thấy tài nguyên.",
                (object?)new { detail = nex.Message }),

            ForbiddenException fex => (
                StatusCodes.Status403Forbidden,
                "Không có quyền truy cập.",
                (object?)new { detail = fex.Message }),

            UnauthorizedAccessException => (
                StatusCodes.Status401Unauthorized,
                "Chưa xác thực.",
                (object?)null),

            _ => (
                StatusCodes.Status500InternalServerError,
                "Đã xảy ra lỗi không mong muốn.",
                (object?)null)
        };

        var problem = new ProblemDetails
        {
            Status   = statusCode,
            Title    = title,
            Instance = context.Request.Path
        };

        if (extensions is not null)
            foreach (var prop in extensions.GetType().GetProperties())
                problem.Extensions[prop.Name] = prop.GetValue(extensions);

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode  = statusCode;

        return context.Response.WriteAsync(JsonSerializer.Serialize(problem, JsonOptions));
    }
}
