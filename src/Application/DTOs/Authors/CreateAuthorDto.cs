using System.ComponentModel.DataAnnotations;

namespace LibraryAPI.Application.DTOs.Authors;

public record CreateAuthorDto
{
    [Required, MaxLength(50)]
    public string FirstName { get; init; } = "";

    [Required, MaxLength(50)]
    public string LastName { get; init; } = "";

    public string? Bio { get; init; }

    public string? Nationality { get; init; }

    public DateTime? BirthDate { get; init; }
}
