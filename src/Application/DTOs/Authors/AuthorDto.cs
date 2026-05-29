namespace LibraryAPI.Application.DTOs.Authors;

public record AuthorDto
{
    public int Id { get; init; }

    public string FullName { get; init; } = "";

    public string? Nationality { get; init; }

    public int BookCount { get; init; }
}
