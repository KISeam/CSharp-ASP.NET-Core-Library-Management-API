using System.ComponentModel.DataAnnotations;
using LibraryAPI.Domain.Enums;

namespace LibraryAPI.Application.DTOs.Books;

public record UpdateBookDto
{
    [MaxLength(200)]
    public string? Title { get; init; }

    public string? Description { get; init; }

    [Range(1, 1000)]
    public int? TotalCopies { get; init; }

    [Range(0, 9999.99)]
    public decimal? FinePerDay { get; init; }

    public Genre? Genre { get; init; }

    public BookStatus? Status { get; init; }
}
