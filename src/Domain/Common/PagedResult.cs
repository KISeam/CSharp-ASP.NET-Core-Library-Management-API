namespace LibraryAPI.Domain.Common;

public class PagedResult<T>
{
    public IEnumerable<T> Items { get; init; } =
        Enumerable.Empty<T>();

    public int TotalCount { get; init; }

    public int Page { get; init; }

    public int PageSize { get; init; }

    public int TotalPages =>
        (int)Math.Ceiling(
            TotalCount / (double)PageSize);

    public bool HasNext => Page < TotalPages;

    public bool HasPrevious => Page > 1;

    public static PagedResult<T> Create(
        IEnumerable<T> items,
        int total,
        int page,
        int size)
    {
        return new PagedResult<T>
        {
            Items = items,
            TotalCount = total,
            Page = page,
            PageSize = size
        };
    }
}
