namespace LibraryAPI.Application.DTOs.Members;

public record MemberDto
{
    public int Id { get; init; }

    public string FullName { get; init; } = "";

    public string Email { get; init; } = "";

    public string Role { get; init; } = "";

    public bool IsActive { get; init; }

    public DateTime CreatedAt { get; init; }
}
