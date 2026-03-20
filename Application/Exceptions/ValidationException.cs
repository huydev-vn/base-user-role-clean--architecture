using FluentValidation.Results;

namespace Application.Exceptions;

/// <summary>
/// Ném khi validation thất bại — Controller bắt và trả 400 với danh sách lỗi.
/// </summary>
public class ValidationException(IEnumerable<ValidationFailure> failures)
    : Exception("One or more validation failures occurred.")
{
    public IDictionary<string, string[]> Errors { get; } =
        failures
            .GroupBy(f => f.PropertyName, f => f.ErrorMessage)
            .ToDictionary(g => g.Key, g => g.ToArray());
}