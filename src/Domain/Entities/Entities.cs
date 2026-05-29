using LibraryAPI.Domain.Enums;

namespace LibraryAPI.Domain.Entities;

// ─────────────────────────────────────────────────────────────
// BASE ENTITY  — every table gets audit fields for free
// ─────────────────────────────────────────────────────────────
public abstract class BaseEntity
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; } = false;   // soft-delete
}

// ─────────────────────────────────────────────────────────────
// USER  — supports multiple roles via UserRole enum
// ─────────────────────────────────────────────────────────────
public class User : BaseEntity
{
    public string FirstName   { get; set; } = "";
    public string LastName    { get; set; } = "";
    public string Email       { get; set; } = "";        // unique index
    public string PhoneNumber { get; set; } = "";
    public string PasswordHash { get; set; } = "";       // BCrypt hash
    public UserRole Role      { get; set; } = UserRole.Member;
    public bool IsActive      { get; set; } = true;
    public DateTime? LastLoginAt { get; set; }

    // navigation — a member can have many borrow records
    public ICollection<BorrowRecord> BorrowRecords { get; set; } = new List<BorrowRecord>();

    // computed — never stored
    public string FullName => $"{FirstName} {LastName}";
}

// ─────────────────────────────────────────────────────────────
// AUTHOR
// ─────────────────────────────────────────────────────────────
public class Author : BaseEntity
{
    public string FirstName   { get; set; } = "";
    public string LastName    { get; set; } = "";
    public string? Bio        { get; set; }
    public string? Nationality { get; set; }
    public DateTime? BirthDate { get; set; }

    public ICollection<Book> Books { get; set; } = new List<Book>();

    public string FullName => $"{FirstName} {LastName}";
}

// ─────────────────────────────────────────────────────────────
// BOOK  — one author, one genre, many copies tracked via status
// ─────────────────────────────────────────────────────────────
public class Book : BaseEntity
{
    public string Title       { get; set; } = "";
    public string ISBN        { get; set; } = "";        // unique, 13-char
    public string? Description { get; set; }
    public int    PublishedYear { get; set; }
    public int    TotalCopies  { get; set; } = 1;
    public int    AvailableCopies { get; set; } = 1;
    public decimal Fine       { get; set; } = 5.00m;    // per-day overdue fine
    public Genre  Genre       { get; set; } = Genre.Other;
    public BookStatus Status  { get; set; } = BookStatus.Available;

    // FK + navigation
    public int    AuthorId    { get; set; }
    public Author Author      { get; set; } = null!;

    public ICollection<BorrowRecord> BorrowRecords { get; set; } = new List<BorrowRecord>();
}

// ─────────────────────────────────────────────────────────────
// BORROW RECORD  — the join between User and Book + business state
// ─────────────────────────────────────────────────────────────
public class BorrowRecord : BaseEntity
{
    public int    UserId       { get; set; }
    public User   User         { get; set; } = null!;

    public int    BookId       { get; set; }
    public Book   Book         { get; set; } = null!;

    public DateTime BorrowedAt { get; set; } = DateTime.UtcNow;
    public DateTime DueDate    { get; set; }                    // BorrowedAt + policy days
    public DateTime? ReturnedAt { get; set; }

    public BorrowStatus Status { get; set; } = BorrowStatus.Active;
    public decimal FineAmount  { get; set; } = 0m;             // calculated on return
    public bool    FinePaid    { get; set; } = false;

    // ── DSA: computed overdue days without storing ──────────
    public int OverdueDays
    {
        get
        {
            if (Status != BorrowStatus.Active && Status != BorrowStatus.Overdue)
                return 0;
            var reference = ReturnedAt ?? DateTime.UtcNow;
            var diff = (reference - DueDate).Days;
            return diff > 0 ? diff : 0;
        }
    }
}

// ─────────────────────────────────────────────────────────────
// REFRESH TOKEN  — stored server-side for JWT rotation
// ─────────────────────────────────────────────────────────────
public class RefreshToken : BaseEntity
{
    public int    UserId    { get; set; }
    public User   User      { get; set; } = null!;
    public string Token     { get; set; } = "";
    public DateTime ExpiresAt { get; set; }
    public bool   IsRevoked { get; set; } = false;
    public string? ReplacedByToken { get; set; }
}
