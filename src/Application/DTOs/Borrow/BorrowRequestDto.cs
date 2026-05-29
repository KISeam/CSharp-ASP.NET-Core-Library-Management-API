using System.ComponentModel.DataAnnotations;

namespace LibraryAPI.Application.DTOs.Borrow;

public record BorrowRequestDto
{
    [Required]
    public int BookId { get; init; }
}
