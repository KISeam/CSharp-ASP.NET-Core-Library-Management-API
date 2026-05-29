namespace LibraryAPI.Application.DTOs.Books;

public record BookDetailDto : BookSummaryDto
{
    public string? Description { get; init; }

    public int PublishedYear { get; init; }

    public int TotalCopies { get; init; }

    public decimal FinePerDay { get; init; }

    public DateTime CreatedAt { get; init; }
}
