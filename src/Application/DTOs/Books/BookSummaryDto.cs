using LibraryAPI.Domain.Enums;

namespace LibraryAPI.Application.DTOs.Books;

public record BookSummaryDto
{
    public int Id { get; init; }

    public string Title { get; init; } = "";

    public string ISBN { get; init; } = "";

    public string AuthorName { get; init; } = "";

    public Genre Genre { get; init; }

    public int AvailableCopies { get; init; }

    public BookStatus Status { get; init; }
}
