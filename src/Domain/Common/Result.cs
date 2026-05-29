namespace LibraryAPI.Domain.Common;

public class Result<T>
{
    public bool IsSuccess { get; }

    public T? Value { get; }

    public string Error { get; }

    public int StatusCode { get; }

    protected Result(
        bool success,
        T? value,
        string error,
        int code)
    {
        IsSuccess = success;
        Value = value;
        Error = error;
        StatusCode = code;
    }

    public static Result<T> Success(T value)
        => new(true, value, string.Empty, 200);

    public static Result<T> Created(T value)
        => new(true, value, string.Empty, 201);

    public static Result<T> Failure(
        string error,
        int statusCode = 400)
        => new(false, default, error, statusCode);

    public static Result<T> NotFound(string resource)
        => new(false, default, $"{resource} not found.", 404);

    public static Result<T> Unauthorized(
        string reason = "Unauthorized.")
        => new(false, default, reason, 401);

    public static Result<T> Forbidden(
        string reason = "Access denied.")
        => new(false, default, reason, 403);

    public static Result<T> Conflict(string message)
        => new(false, default, message, 409);
}
