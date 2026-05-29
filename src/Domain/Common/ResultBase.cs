namespace LibraryAPI.Domain.Common;

public class Result : Result<object?>
{
    private Result(
        bool success,
        string error,
        int code)
        : base(success, null, error, code)
    {
    }

    public static new Result Success()
        => new(true, string.Empty, 200);

    public static new Result Failure(
        string error,
        int code = 400)
        => new(false, error, code);

    public static new Result NotFound(
        string resource)
        => new(false, $"{resource} not found.", 404);
}
