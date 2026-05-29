using System.ComponentModel.DataAnnotations;

namespace LibraryAPI.Application.DTOs.Auth;

public record RegisterRequestDto
{
    [Required, MaxLength(50)]
    public string FirstName { get; init; } = "";

    [Required, MaxLength(50)]
    public string LastName { get; init; } = "";

    [Required, EmailAddress, MaxLength(100)]
    public string Email { get; init; } = "";

    [Required, MinLength(8), MaxLength(100)]
    public string Password { get; init; } = "";

    [Phone]
    public string? PhoneNumber { get; init; }
}
