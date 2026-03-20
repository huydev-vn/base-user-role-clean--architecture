using System.Net;
using System.Text.Json;
using Application.Exceptions;
using ValidationException = Application.Exceptions.ValidationException;

namespace API.Middleware;

/// <summary>
/// Bắt toàn bộ exception chưa được xử lý, map sang HTTP status code phù hợp.
/// Không cần try/catch trong bất kỳ Controller nào.
/// </summary>
public class GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, errors) = exception switch
        {
            ValidationException vex => (HttpStatusCode.BadRequest, vex.Errors),
            NotFoundException => (HttpStatusCode.NotFound, SingleError(exception.Message)),
            ForbiddenException => (HttpStatusCode.Forbidden, SingleError(exception.Message)),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, SingleError("Unauthorized.")),
            _ => (HttpStatusCode.InternalServerError, SingleError("An unexpected error occurred."))
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = new
        {
            status = (int)statusCode,
            errors
        };

        return context.Response.WriteAsync(
            JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            })
        );
    }

    private static IDictionary<string, string[]> SingleError(string message)
        => new Dictionary<string, string[]> { ["general"] = [message] };
}
