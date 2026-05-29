namespace LibraryAPI.Application.DTOs.Auth;

public record UserInfoDto
{
    public int Id { get; init; }

    public string FullName { get; init; } = "";

    public string Email { get; init; } = "";

    public string Role { get; init; } = "";
}
