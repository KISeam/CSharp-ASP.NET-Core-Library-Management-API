namespace LibraryAPI.Domain.Common;

public class QueryParameters
{
    private const int MaxPageSize = 50;

    private int _pageSize = 10;

    public int Page { get; set; } = 1;

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize =
            value > MaxPageSize
                ? MaxPageSize
                : value;
    }

    public string? SearchTerm { get; set; }

    public string? SortBy { get; set; }

    public bool SortDesc { get; set; } = false;
}
