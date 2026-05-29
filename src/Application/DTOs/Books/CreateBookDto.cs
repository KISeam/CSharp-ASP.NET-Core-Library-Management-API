using System.ComponentModel.DataAnnotations;
using LibraryAPI.Domain.Enums;

namespace LibraryAPI.Application.DTOs.Books;

public record CreateBookDto
{
    [Required, MaxLength(200)]
    public string Title { get; init; } = "";

    [Required, StringLength(13, MinimumLength = 10)]
    public string ISBN { get; init; } = "";

    public string? Description { get; init; }

    [Range(1000, 2100)]
    public int PublishedYear { get; init; }

    [Range(1, 1000)]
    public int TotalCopies { get; init; } = 1;

    [Range(0, 9999.99)]
    public decimal FinePerDay { get; init; } = 5m;

    public Genre Genre { get; init; } = Genre.Other;

    [Required]
    public int AuthorId { get; init; }
}
