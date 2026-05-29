using LibraryAPI.Domain.Enums;

namespace LibraryAPI.Application.DTOs.Borrow;

public record BorrowResponseDto
{
    public int Id { get; init; }

    public string BookTitle { get; init; } = "";

    public string MemberName { get; init; } = "";

    public DateTime BorrowedAt { get; init; }

    public DateTime DueDate { get; init; }

    public DateTime? ReturnedAt { get; init; }

    public BorrowStatus Status { get; init; }

    public decimal FineAmount { get; init; }

    public int OverdueDays { get; init; }
}
