namespace LibraryAPI.Domain.Common;

public class NotFoundException : DomainException
{
    public NotFoundException(
        string resource,
        object key)
        : base(
            $"{resource} with id '{key}' was not found.",
            404)
    {
    }
}
