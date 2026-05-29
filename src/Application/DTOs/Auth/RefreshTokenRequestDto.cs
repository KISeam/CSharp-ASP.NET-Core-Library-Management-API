using System.ComponentModel.DataAnnotations;

namespace LibraryAPI.Application.DTOs.Auth;

public record RefreshTokenRequestDto
{
    [Required]
    public string RefreshToken { get; init; } = "";
}
