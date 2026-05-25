namespace Application.Common;

public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? Error { get; }
    public ErrorType ErrorType { get; }

    private Result(T value)
    {
        IsSuccess = true;
        Value = value;
        ErrorType = ErrorType.None;
    }

    private Result(string error, ErrorType errorType)
    {
        IsSuccess = false;
        Error = error;
        ErrorType = errorType;
    }

    public static Result<T> Success(T value) => new(value);

    public static Result<T> Failure(string error, ErrorType errorType = ErrorType.Validation)
        => new(error, errorType);

    public static Result<T> NotFound(string error)     => Failure(error, ErrorType.NotFound);
    public static Result<T> Conflict(string error)     => Failure(error, ErrorType.Conflict);
    public static Result<T> Unauthorized(string error) => Failure(error, ErrorType.Unauthorized);
    public static Result<T> Forbidden(string error)    => Failure(error, ErrorType.Forbidden);
}

public class Result
{
    public bool IsSuccess { get; }
    public string? Error { get; }
    public ErrorType ErrorType { get; }

    private Result(bool isSuccess, string? error, ErrorType errorType)
    {
        IsSuccess = isSuccess;
        Error = error;
        ErrorType = errorType;
    }

    public static Result Success() => new(true, null, ErrorType.None);

    public static Result Failure(string error, ErrorType errorType = ErrorType.Validation)
        => new(false, error, errorType);

    public static Result NotFound(string error)     => Failure(error, ErrorType.NotFound);
    public static Result Conflict(string error)     => Failure(error, ErrorType.Conflict);
    public static Result Unauthorized(string error) => Failure(error, ErrorType.Unauthorized);
    public static Result Forbidden(string error)    => Failure(error, ErrorType.Forbidden);
}
