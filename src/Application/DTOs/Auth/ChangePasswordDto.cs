using System.ComponentModel.DataAnnotations;

namespace LibraryAPI.Application.DTOs.Auth;

public record ChangePasswordDto
{
    [Required]
    public string CurrentPassword { get; init; } = "";

    [Required, MinLength(8)]
    public string NewPassword { get; init; } = "";
}
