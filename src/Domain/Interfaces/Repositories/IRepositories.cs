using LibraryAPI.Domain.Common;
using LibraryAPI.Domain.Entities;
using LibraryAPI.Domain.Enums;

namespace LibraryAPI.Domain.Interfaces.Repositories;

// ─────────────────────────────────────────────────────────────
// GENERIC REPOSITORY — DSA note: wraps EF's change tracker;
// internally uses IQueryable<T> which maps to SQL WHERE clauses.
// ─────────────────────────────────────────────────────────────
public interface IGenericRepository<T> where T : BaseEntity
{
    Task<T?>                GetByIdAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<T>>    GetAllAsync(CancellationToken ct = default);
    Task<PagedResult<T>>    GetPagedAsync(QueryParameters query, CancellationToken ct = default);
    Task                    AddAsync(T entity, CancellationToken ct = default);
    void                    Update(T entity);
    void                    SoftDelete(T entity);          // sets IsDeleted = true
    void                    HardDelete(T entity);
    Task<bool>              ExistsAsync(int id, CancellationToken ct = default);
    Task<int>               CountAsync(CancellationToken ct = default);
}

// ─────────────────────────────────────────────────────────────
// SPECIFIC REPOS — add domain-query methods on top of generic
// ─────────────────────────────────────────────────────────────
public interface IUserRepository : IGenericRepository<User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<IEnumerable<User>> GetByRoleAsync(UserRole role, CancellationToken ct = default);
    Task<bool>  EmailExistsAsync(string email, CancellationToken ct = default);
    Task<User?> GetWithBorrowsAsync(int userId, CancellationToken ct = default);
}

public interface IBookRepository : IGenericRepository<Book>
{
    Task<Book?> GetByIsbnAsync(string isbn, CancellationToken ct = default);
    Task<Book?> GetWithAuthorAsync(int bookId, CancellationToken ct = default);
    Task<IEnumerable<Book>> SearchAsync(string term, CancellationToken ct = default);
    Task<IEnumerable<Book>> GetByGenreAsync(Genre genre, CancellationToken ct = default);
    Task<IEnumerable<Book>> GetByAuthorAsync(int authorId, CancellationToken ct = default);
    Task<IEnumerable<Book>> GetAvailableBooksAsync(CancellationToken ct = default);

    // DSA: binary search on ISBN-sorted in-memory index
    Book? BinarySearchByIsbn(IList<Book> sortedBooks, string isbn);
}

public interface IAuthorRepository : IGenericRepository<Author>
{
    Task<Author?> GetWithBooksAsync(int authorId, CancellationToken ct = default);
    Task<IEnumerable<Author>> SearchByNameAsync(string name, CancellationToken ct = default);
}

public interface IBorrowRepository : IGenericRepository<BorrowRecord>
{
    Task<BorrowRecord?> GetActiveByUserAndBookAsync(int userId, int bookId, CancellationToken ct = default);
    Task<IEnumerable<BorrowRecord>> GetByUserAsync(int userId, CancellationToken ct = default);
    Task<IEnumerable<BorrowRecord>> GetOverdueAsync(CancellationToken ct = default);
    Task<IEnumerable<BorrowRecord>> GetActiveBorrowsAsync(CancellationToken ct = default);
    Task<BorrowRecord?> GetWithDetailsAsync(int id, CancellationToken ct = default);
}

public interface IRefreshTokenRepository : IGenericRepository<RefreshToken>
{
    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken ct = default);
    Task RevokeAllForUserAsync(int userId, CancellationToken ct = default);
}

// ─────────────────────────────────────────────────────────────
// UNIT OF WORK — ensures atomicity: all-or-nothing DB commits
// ─────────────────────────────────────────────────────────────
public interface IUnitOfWork : IDisposable
{
    IUserRepository         Users         { get; }
    IBookRepository         Books         { get; }
    IAuthorRepository       Authors       { get; }
    IBorrowRepository       Borrows       { get; }
    IRefreshTokenRepository RefreshTokens { get; }

    Task<int> SaveChangesAsync(CancellationToken ct = default);
    Task BeginTransactionAsync(CancellationToken ct = default);
    Task CommitTransactionAsync(CancellationToken ct = default);
    Task RollbackTransactionAsync(CancellationToken ct = default);
}
