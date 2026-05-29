namespace LibraryAPI.Application.DTOs.Members;

public record MemberDetailDto : MemberDto
{
    public string? PhoneNumber { get; init; }

    public DateTime? LastLoginAt { get; init; }

    public int ActiveBorrows { get; init; }

    public int TotalBorrows { get; init; }

    public decimal OutstandingFines { get; init; }
}
