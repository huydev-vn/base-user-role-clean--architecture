using FluentValidation;
using MediatR;

namespace Application.Behaviors;

/// <summary>
/// Pipeline Behavior chạy SAU Logging, TRƯỚC Handler.
/// Tự động chạy tất cả FluentValidation validators đã đăng ký cho TRequest.
/// Nếu validation thất bại → ném ValidationException → GlobalExceptionMiddleware bắt → trả 400.
/// Không có validator nào được đăng ký → bỏ qua, chạy thẳng vào Handler.
/// </summary>
public sealed class ValidationBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any())
            return await next(cancellationToken);

        var context = new ValidationContext<TRequest>(request);

        var failures = validators
            .Select(v => v.Validate(context))
            .SelectMany(result => result.Errors)
            .Where(failure => failure is not null)
            .ToList();

        if (failures.Count > 0)
            throw new Application.Exceptions.ValidationException(failures);

        return await next(cancellationToken);
    }
}
