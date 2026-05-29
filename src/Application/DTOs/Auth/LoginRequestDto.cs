using System.ComponentModel.DataAnnotations;

namespace LibraryAPI.Application.DTOs.Auth;

public record LoginRequestDto
{
    [Required, EmailAddress]
    public string Email { get; init; } = "";

    [Required]
    public string Password { get; init; } = "";
}
