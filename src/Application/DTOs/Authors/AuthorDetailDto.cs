using LibraryAPI.Application.DTOs.Books;

namespace LibraryAPI.Application.DTOs.Authors;

public record AuthorDetailDto : AuthorDto
{
    public string? Bio { get; init; }

    public DateTime? BirthDate { get; init; }

    public IEnumerable<BookSummaryDto> Books { get; init; } =
        Enumerable.Empty<BookSummaryDto>();
}
