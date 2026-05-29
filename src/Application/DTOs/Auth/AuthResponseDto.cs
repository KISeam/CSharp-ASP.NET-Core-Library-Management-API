namespace LibraryAPI.Application.DTOs.Auth;

public record AuthResponseDto
{
    public string AccessToken { get; init; } = "";
    public string RefreshToken { get; init; } = "";
    public DateTime ExpiresAt { get; init; }

    public UserInfoDto User { get; init; } = null!;
}
