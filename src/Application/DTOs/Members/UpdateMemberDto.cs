using System.ComponentModel.DataAnnotations;

namespace LibraryAPI.Application.DTOs.Members;

public record UpdateMemberDto
{
    [MaxLength(50)]
    public string? FirstName { get; init; }

    [MaxLength(50)]
    public string? LastName { get; init; }

    [Phone]
    public string? PhoneNumber { get; init; }
}
