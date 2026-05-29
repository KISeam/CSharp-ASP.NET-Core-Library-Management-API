namespace LibraryAPI.Application.DTOs.Borrow;

public record FineCalculationDto
{
    public int BorrowId { get; init; }

    public string BookTitle { get; init; } = "";

    public int OverdueDays { get; init; }

    public decimal FinePerDay { get; init; }

    public decimal TotalFine { get; init; }
}
