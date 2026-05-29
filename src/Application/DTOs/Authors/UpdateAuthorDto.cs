namespace LibraryAPI.Application.DTOs.Authors;

public record UpdateAuthorDto
{
    public string? FirstName { get; init; }

    public string? LastName { get; init; }

    public string? Bio { get; init; }

    public string? Nationality { get; init; }

    public DateTime? BirthDate { get; init; }
}
