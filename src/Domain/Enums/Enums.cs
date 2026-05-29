namespace LibraryAPI.Domain.Enums;

/// <summary>System roles — drives all authorization decisions.</summary>
public enum UserRole
{
    Admin     = 1,   // full system access
    Librarian = 2,   // book & borrow management
    Member    = 3    // self-service only
}

/// <summary>Lifecycle state of a single book copy.</summary>
public enum BookStatus
{
    Available   = 1,
    Borrowed    = 2,
    Reserved    = 3,
    Maintenance = 4,
    Lost        = 5
}

/// <summary>State of a borrow transaction.</summary>
public enum BorrowStatus
{
    Active    = 1,   // currently borrowed
    Returned  = 2,   // returned on time
    Overdue   = 3,   // past due date, not yet returned
    LostPaid  = 4    // member paid lost-book fine
}

/// <summary>Genre taxonomy — used for LINQ filtering.</summary>
public enum Genre
{
    Fiction        = 1,
    NonFiction     = 2,
    Science        = 3,
    Technology     = 4,
    History        = 5,
    Biography      = 6,
    Children       = 7,
    Academic       = 8,
    SelfHelp       = 9,
    Other          = 10
}
