using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using LibraryAPI.Domain.Common;
using LibraryAPI.Domain.Entities;
using LibraryAPI.Domain.Enums;
using LibraryAPI.Domain.Interfaces.Repositories;
using LibraryAPI.Infrastructure.Data;

namespace LibraryAPI.Infrastructure.Repositories;

// ─────────────────────────────────────────────────────────────
// GENERIC REPOSITORY
// ─────────────────────────────────────────────────────────────
public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
{
    protected readonly LibraryDbContext _db;
    protected readonly DbSet<T> _set;

    public GenericRepository(LibraryDbContext db)
    {
        _db  = db;
        _set = db.Set<T>();
    }

    public async Task<T?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _set.FirstOrDefaultAsync(e => e.Id == id, ct);

    public async Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default)
        => await _set.ToListAsync(ct);

    public async Task<PagedResult<T>> GetPagedAsync(QueryParameters q, CancellationToken ct = default)
    {
        var query = _set.AsQueryable();
        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((q.Page - 1) * q.PageSize)
            .Take(q.PageSize)
            .ToListAsync(ct);
        return PagedResult<T>.Create(items, total, q.Page, q.PageSize);
    }

    public async Task AddAsync(T entity, CancellationToken ct = default)
        => await _set.AddAsync(entity, ct);

    public void Update(T entity)
        => _set.Update(entity);

    public void SoftDelete(T entity)
    {
        entity.IsDeleted = true;
        _set.Update(entity);
    }

    public void HardDelete(T entity)
        => _set.Remove(entity);

    public async Task<bool> ExistsAsync(int id, CancellationToken ct = default)
        => await _set.AnyAsync(e => e.Id == id, ct);

    public async Task<int> CountAsync(CancellationToken ct = default)
        => await _set.CountAsync(ct);
}

// ─────────────────────────────────────────────────────────────
// USER REPOSITORY
// ─────────────────────────────────────────────────────────────
public class UserRepository : GenericRepository<User>, IUserRepository
{
    public UserRepository(LibraryDbContext db) : base(db) { }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
        => await _set.FirstOrDefaultAsync(u => u.Email == email.ToLower(), ct);

    public async Task<IEnumerable<User>> GetByRoleAsync(UserRole role, CancellationToken ct = default)
        => await _set.Where(u => u.Role == role).ToListAsync(ct);

    public async Task<bool> EmailExistsAsync(string email, CancellationToken ct = default)
        => await _set.AnyAsync(u => u.Email == email.ToLower(), ct);

    public async Task<User?> GetWithBorrowsAsync(int userId, CancellationToken ct = default)
        => await _set
            .Include(u => u.BorrowRecords)
                .ThenInclude(br => br.Book)
            .FirstOrDefaultAsync(u => u.Id == userId, ct);
}

// ─────────────────────────────────────────────────────────────
// BOOK REPOSITORY  — includes DSA binary search
// ─────────────────────────────────────────────────────────────
public class BookRepository : GenericRepository<Book>, IBookRepository
{
    public BookRepository(LibraryDbContext db) : base(db) { }

    public async Task<Book?> GetByIsbnAsync(string isbn, CancellationToken ct = default)
        => await _set.Include(b => b.Author)
                     .FirstOrDefaultAsync(b => b.ISBN == isbn, ct);

    public async Task<Book?> GetWithAuthorAsync(int bookId, CancellationToken ct = default)
        => await _set.Include(b => b.Author)
                     .FirstOrDefaultAsync(b => b.Id == bookId, ct);

    public async Task<IEnumerable<Book>> SearchAsync(string term, CancellationToken ct = default)
    {
        var lower = term.ToLower();
        return await _set
            .Include(b => b.Author)
            .Where(b => b.Title.ToLower().Contains(lower)
                     || b.ISBN.Contains(lower)
                     || b.Author.FirstName.ToLower().Contains(lower)
                     || b.Author.LastName.ToLower().Contains(lower))
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<Book>> GetByGenreAsync(Genre genre, CancellationToken ct = default)
        => await _set.Include(b => b.Author)
                     .Where(b => b.Genre == genre)
                     .ToListAsync(ct);

    public async Task<IEnumerable<Book>> GetByAuthorAsync(int authorId, CancellationToken ct = default)
        => await _set.Where(b => b.AuthorId == authorId).ToListAsync(ct);

    public async Task<IEnumerable<Book>> GetAvailableBooksAsync(CancellationToken ct = default)
        => await _set.Include(b => b.Author)
                     .Where(b => b.AvailableCopies > 0)
                     .ToListAsync(ct);

    /// <summary>
    /// DSA — Binary Search O(log n).
    /// Caller must pass an ISBN-sorted list (e.g., retrieved once and cached).
    /// </summary>
    public Book? BinarySearchByIsbn(IList<Book> sortedBooks, string isbn)
    {
        int lo = 0, hi = sortedBooks.Count - 1;
        while (lo <= hi)
        {
            int mid = lo + (hi - lo) / 2;
            int cmp = string.Compare(sortedBooks[mid].ISBN, isbn, StringComparison.Ordinal);
            if      (cmp == 0) return sortedBooks[mid];
            else if (cmp <  0) lo = mid + 1;
            else               hi = mid - 1;
        }
        return null;
    }
}

// ─────────────────────────────────────────────────────────────
// AUTHOR REPOSITORY
// ─────────────────────────────────────────────────────────────
public class AuthorRepository : GenericRepository<Author>, IAuthorRepository
{
    public AuthorRepository(LibraryDbContext db) : base(db) { }

    public async Task<Author?> GetWithBooksAsync(int authorId, CancellationToken ct = default)
        => await _set.Include(a => a.Books)
                     .FirstOrDefaultAsync(a => a.Id == authorId, ct);

    public async Task<IEnumerable<Author>> SearchByNameAsync(string name, CancellationToken ct = default)
    {
        var lower = name.ToLower();
        return await _set
            .Where(a => a.FirstName.ToLower().Contains(lower)
                     || a.LastName.ToLower().Contains(lower))
            .ToListAsync(ct);
    }
}

// ─────────────────────────────────────────────────────────────
// BORROW REPOSITORY
// ─────────────────────────────────────────────────────────────
public class BorrowRepository : GenericRepository<BorrowRecord>, IBorrowRepository
{
    public BorrowRepository(LibraryDbContext db) : base(db) { }

    public async Task<BorrowRecord?> GetActiveByUserAndBookAsync(
        int userId, int bookId, CancellationToken ct = default)
        => await _set.FirstOrDefaultAsync(
            br => br.UserId == userId
               && br.BookId == bookId
               && br.Status == BorrowStatus.Active, ct);

    public async Task<IEnumerable<BorrowRecord>> GetByUserAsync(int userId, CancellationToken ct = default)
        => await _set.Include(br => br.Book)
                     .Where(br => br.UserId == userId)
                     .OrderByDescending(br => br.BorrowedAt)
                     .ToListAsync(ct);

    public async Task<IEnumerable<BorrowRecord>> GetOverdueAsync(CancellationToken ct = default)
        => await _set.Include(br => br.Book)
                     .Include(br => br.User)
                     .Where(br => br.DueDate < DateTime.UtcNow
                               && br.Status == BorrowStatus.Active)
                     .ToListAsync(ct);

    public async Task<IEnumerable<BorrowRecord>> GetActiveBorrowsAsync(CancellationToken ct = default)
        => await _set.Include(br => br.Book)
                     .Include(br => br.User)
                     .Where(br => br.Status == BorrowStatus.Active)
                     .ToListAsync(ct);

    public async Task<BorrowRecord?> GetWithDetailsAsync(int id, CancellationToken ct = default)
        => await _set.Include(br => br.Book)
                     .Include(br => br.User)
                     .FirstOrDefaultAsync(br => br.Id == id, ct);
}

// ─────────────────────────────────────────────────────────────
// REFRESH TOKEN REPOSITORY
// ─────────────────────────────────────────────────────────────
public class RefreshTokenRepository
    : GenericRepository<RefreshToken>, IRefreshTokenRepository
{
    public RefreshTokenRepository(LibraryDbContext db) : base(db) { }

    public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken ct = default)
        => await _set.Include(rt => rt.User)
                     .FirstOrDefaultAsync(rt => rt.Token == token, ct);

    public async Task RevokeAllForUserAsync(int userId, CancellationToken ct = default)
    {
        var tokens = await _set
            .Where(rt => rt.UserId == userId && !rt.IsRevoked)
            .ToListAsync(ct);
        tokens.ForEach(t => t.IsRevoked = true);
    }
}

// ─────────────────────────────────────────────────────────────
// UNIT OF WORK
// ─────────────────────────────────────────────────────────────
public class UnitOfWork : IUnitOfWork
{
    private readonly LibraryDbContext _db;
    private IDbContextTransaction?   _transaction;

    public IUserRepository         Users         { get; }
    public IBookRepository         Books         { get; }
    public IAuthorRepository       Authors       { get; }
    public IBorrowRepository       Borrows       { get; }
    public IRefreshTokenRepository RefreshTokens { get; }

    public UnitOfWork(LibraryDbContext db)
    {
        _db           = db;
        Users         = new UserRepository(db);
        Books         = new BookRepository(db);
        Authors       = new AuthorRepository(db);
        Borrows       = new BorrowRepository(db);
        RefreshTokens = new RefreshTokenRepository(db);
    }

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);

    public async Task BeginTransactionAsync(CancellationToken ct = default)
        => _transaction = await _db.Database.BeginTransactionAsync(ct);

    public async Task CommitTransactionAsync(CancellationToken ct = default)
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync(ct);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken ct = default)
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync(ct);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _db.Dispose();
    }
}
