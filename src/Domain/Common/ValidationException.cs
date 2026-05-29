namespace LibraryAPI.Domain.Common;

public class ValidationException : DomainException
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException(
        IDictionary<string, string[]> errors)
        : base(
            "One or more validation errors occurred.",
            422)
    {
        Errors = errors;
    }
}
