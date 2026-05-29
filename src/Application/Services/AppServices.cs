using LibraryAPI.Domain.Common;
using LibraryAPI.Domain.Entities;
using LibraryAPI.Domain.Enums;
using LibraryAPI.Domain.Interfaces.Repositories;
using LibraryAPI.Domain.Interfaces.Services;
using LibraryAPI.Application.DTOs.Books;
using LibraryAPI.Application.DTOs.Authors;
using LibraryAPI.Application.DTOs.Members;
using LibraryAPI.Application.DTOs.Borrow;
using Microsoft.Extensions.Configuration;

namespace LibraryAPI.Application.Services;

// ─────────────────────────────────────────────────────────────
// BOOK SERVICE
// ─────────────────────────────────────────────────────────────
public class BookService : IBookService
{
    private readonly IUnitOfWork _uow;

    public BookService(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<PagedResult<BookSummaryDto>>> GetAllAsync(
        BookQueryParameters q, CancellationToken ct = default)
    {
        var books = await _uow.Books.GetAvailableBooksAsync(ct);

        // LINQ pipeline: filter → sort → page
        IEnumerable<Book> query = books;

        if (q.Genre.HasValue)
            query = query.Where(b => b.Genre == q.Genre.Value);

        if (q.AuthorId.HasValue)
            query = query.Where(b => b.AuthorId == q.AuthorId.Value);

        if (q.OnlyAvailable)
            query = query.Where(b => b.AvailableCopies > 0);

        if (!string.IsNullOrWhiteSpace(q.SearchTerm))
        {
            var term = q.SearchTerm.ToLower();
            query = query.Where(b =>
                b.Title.ToLower().Contains(term) ||
                b.ISBN.Contains(term) ||
                b.Author.FullName.ToLower().Contains(term));
        }

        query = q.SortBy?.ToLower() switch
        {
            "title"  => q.SortDesc ? query.OrderByDescending(b => b.Title)  : query.OrderBy(b => b.Title),
            "author" => q.SortDesc ? query.OrderByDescending(b => b.Author.LastName) : query.OrderBy(b => b.Author.LastName),
            "year"   => q.SortDesc ? query.OrderByDescending(b => b.PublishedYear)   : query.OrderBy(b => b.PublishedYear),
            _        => query.OrderBy(b => b.Title)
        };

        var total = query.Count();
        var items = query.Skip((q.Page - 1) * q.PageSize).Take(q.PageSize)
                         .Select(ToSummary).ToList();

        return Result<PagedResult<BookSummaryDto>>.Success(
            PagedResult<BookSummaryDto>.Create(items, total, q.Page, q.PageSize));
    }

    public async Task<Result<BookDetailDto>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var book = await _uow.Books.GetWithAuthorAsync(id, ct);
        return book is null
            ? Result<BookDetailDto>.NotFound("Book")
            : Result<BookDetailDto>.Success(ToDetail(book));
    }

    public async Task<Result<BookDetailDto>> GetByIsbnAsync(string isbn, CancellationToken ct = default)
    {
        var book = await _uow.Books.GetByIsbnAsync(isbn, ct);
        return book is null
            ? Result<BookDetailDto>.NotFound("Book")
            : Result<BookDetailDto>.Success(ToDetail(book));
    }

    public async Task<Result<BookDetailDto>> CreateAsync(CreateBookDto dto, CancellationToken ct = default)
    {
        if (!await _uow.Authors.ExistsAsync(dto.AuthorId, ct))
            return Result<BookDetailDto>.NotFound("Author");

        if (await _uow.Books.GetByIsbnAsync(dto.ISBN, ct) is not null)
            return Result<BookDetailDto>.Conflict($"ISBN '{dto.ISBN}' already exists.");

        var book = new Book
        {
            Title          = dto.Title.Trim(),
            ISBN           = dto.ISBN.Trim(),
            Description    = dto.Description,
            PublishedYear  = dto.PublishedYear,
            TotalCopies    = dto.TotalCopies,
            AvailableCopies = dto.TotalCopies,
            Fine           = dto.FinePerDay,
            Genre          = dto.Genre,
            AuthorId       = dto.AuthorId,
            Status         = BookStatus.Available
        };

        await _uow.Books.AddAsync(book, ct);
        await _uow.SaveChangesAsync(ct);

        var created = await _uow.Books.GetWithAuthorAsync(book.Id, ct);
        return Result<BookDetailDto>.Created(ToDetail(created!));
    }

    public async Task<Result<BookDetailDto>> UpdateAsync(
        int id, UpdateBookDto dto, CancellationToken ct = default)
    {
        var book = await _uow.Books.GetWithAuthorAsync(id, ct);
        if (book is null) return Result<BookDetailDto>.NotFound("Book");

        if (dto.Title       is not null) book.Title     = dto.Title.Trim();
        if (dto.Description is not null) book.Description = dto.Description;
        if (dto.TotalCopies is not null)
        {
            var diff = dto.TotalCopies.Value - book.TotalCopies;
            book.TotalCopies    = dto.TotalCopies.Value;
            book.AvailableCopies = Math.Max(0, book.AvailableCopies + diff);
        }
        if (dto.FinePerDay is not null) book.Fine   = dto.FinePerDay.Value;
        if (dto.Genre      is not null) book.Genre  = dto.Genre.Value;
        if (dto.Status     is not null) book.Status = dto.Status.Value;

        _uow.Books.Update(book);
        await _uow.SaveChangesAsync(ct);
        return Result<BookDetailDto>.Success(ToDetail(book));
    }

    public async Task<Result<bool>> DeleteAsync(int id, CancellationToken ct = default)
    {
        var book = await _uow.Books.GetByIdAsync(id, ct);
        if (book is null) return Result<bool>.NotFound("Book");

        _uow.Books.SoftDelete(book);
        await _uow.SaveChangesAsync(ct);
        return Result<bool>.Success(true);
    }

    public async Task<Result<IEnumerable<BookSummaryDto>>> SearchAsync(
        string term, CancellationToken ct = default)
    {
        var books = await _uow.Books.SearchAsync(term, ct);
        return Result<IEnumerable<BookSummaryDto>>.Success(books.Select(ToSummary));
    }

    // ── Mappers ───────────────────────────────────────────────
    private static BookSummaryDto ToSummary(Book b) => new()
    {
        Id             = b.Id,
        Title          = b.Title,
        ISBN           = b.ISBN,
        AuthorName     = b.Author?.FullName ?? "",
        Genre          = b.Genre,
        AvailableCopies = b.AvailableCopies,
        Status         = b.Status
    };

    private static BookDetailDto ToDetail(Book b) => new()
    {
        Id             = b.Id,
        Title          = b.Title,
        ISBN           = b.ISBN,
        AuthorName     = b.Author?.FullName ?? "",
        Genre          = b.Genre,
        AvailableCopies = b.AvailableCopies,
        TotalCopies    = b.TotalCopies,
        Status         = b.Status,
        Description    = b.Description,
        PublishedYear  = b.PublishedYear,
        FinePerDay     = b.Fine,
        CreatedAt      = b.CreatedAt
    };
}

// ─────────────────────────────────────────────────────────────
// AUTHOR SERVICE
// ─────────────────────────────────────────────────────────────
public class AuthorService : IAuthorService
{
    private readonly IUnitOfWork _uow;
    public AuthorService(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<PagedResult<AuthorDto>>> GetAllAsync(
        QueryParameters q, CancellationToken ct = default)
    {
        var paged = await _uow.Authors.GetPagedAsync(q, ct);
        var dtos  = paged.Items.Select(a => new AuthorDto
        {
            Id          = a.Id,
            FullName    = a.FullName,
            Nationality = a.Nationality,
            BookCount   = a.Books.Count
        });
        return Result<PagedResult<AuthorDto>>.Success(
            PagedResult<AuthorDto>.Create(dtos, paged.TotalCount, paged.Page, paged.PageSize));
    }

    public async Task<Result<AuthorDetailDto>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var author = await _uow.Authors.GetWithBooksAsync(id, ct);
        if (author is null) return Result<AuthorDetailDto>.NotFound("Author");

        return Result<AuthorDetailDto>.Success(new AuthorDetailDto
        {
            Id          = author.Id,
            FullName    = author.FullName,
            Nationality = author.Nationality,
            BookCount   = author.Books.Count,
            Bio         = author.Bio,
            BirthDate   = author.BirthDate,
            Books       = author.Books.Select(b => new BookSummaryDto
            {
                Id             = b.Id,
                Title          = b.Title,
                ISBN           = b.ISBN,
                AuthorName     = author.FullName,
                Genre          = b.Genre,
                AvailableCopies = b.AvailableCopies,
                Status         = b.Status
            })
        });
    }

    public async Task<Result<AuthorDto>> CreateAsync(CreateAuthorDto dto, CancellationToken ct = default)
    {
        var author = new Author
        {
            FirstName   = dto.FirstName.Trim(),
            LastName    = dto.LastName.Trim(),
            Bio         = dto.Bio,
            Nationality = dto.Nationality,
            BirthDate   = dto.BirthDate
        };
        await _uow.Authors.AddAsync(author, ct);
        await _uow.SaveChangesAsync(ct);
        return Result<AuthorDto>.Created(new AuthorDto
        {
            Id = author.Id, FullName = author.FullName,
            Nationality = author.Nationality, BookCount = 0
        });
    }

    public async Task<Result<AuthorDto>> UpdateAsync(
        int id, UpdateAuthorDto dto, CancellationToken ct = default)
    {
        var author = await _uow.Authors.GetByIdAsync(id, ct);
        if (author is null) return Result<AuthorDto>.NotFound("Author");

        if (dto.FirstName   is not null) author.FirstName   = dto.FirstName.Trim();
        if (dto.LastName    is not null) author.LastName    = dto.LastName.Trim();
        if (dto.Bio         is not null) author.Bio         = dto.Bio;
        if (dto.Nationality is not null) author.Nationality = dto.Nationality;
        if (dto.BirthDate   is not null) author.BirthDate   = dto.BirthDate;

        _uow.Authors.Update(author);
        await _uow.SaveChangesAsync(ct);
        return Result<AuthorDto>.Success(new AuthorDto
        {
            Id = author.Id, FullName = author.FullName, Nationality = author.Nationality
        });
    }

    public async Task<Result<bool>> DeleteAsync(int id, CancellationToken ct = default)
    {
        var author = await _uow.Authors.GetByIdAsync(id, ct);
        if (author is null) return Result<bool>.NotFound("Author");
        _uow.Authors.SoftDelete(author);
        await _uow.SaveChangesAsync(ct);
        return Result<bool>.Success(true);
    }
}

// ─────────────────────────────────────────────────────────────
// MEMBER SERVICE
// ─────────────────────────────────────────────────────────────
public class MemberService : IMemberService
{
    private readonly IUnitOfWork _uow;
    public MemberService(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<PagedResult<MemberDto>>> GetAllAsync(
        QueryParameters q, CancellationToken ct = default)
    {
        var paged = await _uow.Users.GetPagedAsync(q, ct);
        var dtos  = paged.Items.Select(ToDto);
        return Result<PagedResult<MemberDto>>.Success(
            PagedResult<MemberDto>.Create(dtos, paged.TotalCount, paged.Page, paged.PageSize));
    }

    public async Task<Result<MemberDetailDto>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var user = await _uow.Users.GetWithBorrowsAsync(id, ct);
        if (user is null) return Result<MemberDetailDto>.NotFound("Member");
        return Result<MemberDetailDto>.Success(ToDetailDto(user));
    }

    public async Task<Result<MemberDetailDto>> GetProfileAsync(int userId, CancellationToken ct = default)
        => await GetByIdAsync(userId, ct);

    public async Task<Result<MemberDto>> UpdateAsync(
        int id, UpdateMemberDto dto, CancellationToken ct = default)
    {
        var user = await _uow.Users.GetByIdAsync(id, ct);
        if (user is null) return Result<MemberDto>.NotFound("Member");

        if (dto.FirstName   is not null) user.FirstName   = dto.FirstName.Trim();
        if (dto.LastName    is not null) user.LastName    = dto.LastName.Trim();
        if (dto.PhoneNumber is not null) user.PhoneNumber = dto.PhoneNumber;

        _uow.Users.Update(user);
        await _uow.SaveChangesAsync(ct);
        return Result<MemberDto>.Success(ToDto(user));
    }

    public async Task<Result<bool>> DeactivateAsync(int id, CancellationToken ct = default)
    {
        var user = await _uow.Users.GetByIdAsync(id, ct);
        if (user is null) return Result<bool>.NotFound("Member");
        user.IsActive = false;
        _uow.Users.Update(user);
        await _uow.SaveChangesAsync(ct);
        return Result<bool>.Success(true);
    }

    private static MemberDto ToDto(User u) => new()
    {
        Id        = u.Id,
        FullName  = u.FullName,
        Email     = u.Email,
        Role      = u.Role.ToString(),
        IsActive  = u.IsActive,
        CreatedAt = u.CreatedAt
    };

    private static MemberDetailDto ToDetailDto(User u) => new()
    {
        Id               = u.Id,
        FullName         = u.FullName,
        Email            = u.Email,
        Role             = u.Role.ToString(),
        IsActive         = u.IsActive,
        CreatedAt        = u.CreatedAt,
        PhoneNumber      = u.PhoneNumber,
        LastLoginAt      = u.LastLoginAt,
        ActiveBorrows    = u.BorrowRecords.Count(br => br.Status == BorrowStatus.Active),
        TotalBorrows     = u.BorrowRecords.Count,
        OutstandingFines = u.BorrowRecords
                            .Where(br => !br.FinePaid && br.FineAmount > 0)
                            .Sum(br => br.FineAmount)
    };
}

// ─────────────────────────────────────────────────────────────
// BORROW SERVICE  — DSA: Priority Queue for fine ranking
// ─────────────────────────────────────────────────────────────
public class BorrowService : IBorrowService
{
    private readonly IUnitOfWork    _uow;
    private readonly IConfiguration _config;
    private readonly int            _maxBorrowDays;
    private readonly int            _maxBooksPerMember;

    public BorrowService(IUnitOfWork uow, IConfiguration config)
    {
        _uow               = uow;
        _config            = config;
        _maxBorrowDays     = int.Parse(config["Library:MaxBorrowDays"]     ?? "14");
        _maxBooksPerMember = int.Parse(config["Library:MaxBooksPerMember"] ?? "5");
    }

    public async Task<Result<BorrowResponseDto>> BorrowBookAsync(
        int userId, BorrowRequestDto dto, CancellationToken ct = default)
    {
        await _uow.BeginTransactionAsync(ct);
        try
        {
            var user = await _uow.Users.GetWithBorrowsAsync(userId, ct);
            if (user is null) return Result<BorrowResponseDto>.NotFound("User");

            var book = await _uow.Books.GetByIdAsync(dto.BookId, ct);
            if (book is null) return Result<BorrowResponseDto>.NotFound("Book");

            // Business rules — fail fast with guard clauses
            if (book.AvailableCopies <= 0)
                return Result<BorrowResponseDto>.Failure("No copies available.", 409);

            var activeBorrows = user.BorrowRecords.Count(br => br.Status == BorrowStatus.Active);
            if (activeBorrows >= _maxBooksPerMember)
                return Result<BorrowResponseDto>.Failure(
                    $"Member cannot borrow more than {_maxBooksPerMember} books at once.", 409);

            var existing = await _uow.Borrows
                .GetActiveByUserAndBookAsync(userId, dto.BookId, ct);
            if (existing is not null)
                return Result<BorrowResponseDto>.Conflict("Member already has this book.");

            var record = new BorrowRecord
            {
                UserId    = userId,
                BookId    = dto.BookId,
                BorrowedAt = DateTime.UtcNow,
                DueDate   = DateTime.UtcNow.AddDays(_maxBorrowDays),
                Status    = BorrowStatus.Active
            };

            book.AvailableCopies--;
            if (book.AvailableCopies == 0) book.Status = BookStatus.Borrowed;

            await _uow.Borrows.AddAsync(record, ct);
            _uow.Books.Update(book);
            await _uow.SaveChangesAsync(ct);
            await _uow.CommitTransactionAsync(ct);

            return Result<BorrowResponseDto>.Created(ToDto(record, book, user));
        }
        catch
        {
            await _uow.RollbackTransactionAsync(ct);
            throw;
        }
    }

    public async Task<Result<BorrowResponseDto>> ReturnBookAsync(
        int userId, int borrowId, CancellationToken ct = default)
    {
        await _uow.BeginTransactionAsync(ct);
        try
        {
            var record = await _uow.Borrows.GetWithDetailsAsync(borrowId, ct);
            if (record is null) return Result<BorrowResponseDto>.NotFound("BorrowRecord");

            if (record.UserId != userId)
                return Result<BorrowResponseDto>.Forbidden("This borrow record doesn't belong to you.");

            if (record.Status != BorrowStatus.Active)
                return Result<BorrowResponseDto>.Failure("Book has already been returned.", 409);

            record.ReturnedAt = DateTime.UtcNow;
            record.Status     = record.OverdueDays > 0
                ? BorrowStatus.Returned   // fine already calculated
                : BorrowStatus.Returned;

            // Fine calculation — overdue days × per-day rate
            if (record.OverdueDays > 0)
            {
                record.FineAmount = record.OverdueDays * record.Book.Fine;
                record.Status     = BorrowStatus.Returned;
            }

            var book = record.Book;
            book.AvailableCopies++;
            if (book.Status == BookStatus.Borrowed && book.AvailableCopies > 0)
                book.Status = BookStatus.Available;

            _uow.Borrows.Update(record);
            _uow.Books.Update(book);
            await _uow.SaveChangesAsync(ct);
            await _uow.CommitTransactionAsync(ct);

            return Result<BorrowResponseDto>.Success(ToDto(record, book, record.User));
        }
        catch
        {
            await _uow.RollbackTransactionAsync(ct);
            throw;
        }
    }

    public async Task<Result<PagedResult<BorrowResponseDto>>> GetAllBorrowsAsync(
        QueryParameters q, CancellationToken ct = default)
    {
        var all   = await _uow.Borrows.GetActiveBorrowsAsync(ct);
        var total = all.Count();
        var items = all.Skip((q.Page - 1) * q.PageSize).Take(q.PageSize)
                       .Select(br => ToDto(br, br.Book, br.User)).ToList();
        return Result<PagedResult<BorrowResponseDto>>.Success(
            PagedResult<BorrowResponseDto>.Create(items, total, q.Page, q.PageSize));
    }

    public async Task<Result<IEnumerable<BorrowResponseDto>>> GetUserBorrowsAsync(
        int userId, CancellationToken ct = default)
    {
        var records = await _uow.Borrows.GetByUserAsync(userId, ct);
        return Result<IEnumerable<BorrowResponseDto>>.Success(
            records.Select(br => ToDto(br, br.Book, br.User)));
    }

    public async Task<Result<IEnumerable<BorrowResponseDto>>> GetOverdueAsync(
        CancellationToken ct = default)
    {
        var records = await _uow.Borrows.GetOverdueAsync(ct);

        // DSA — Priority Queue: surface highest-overdue records first
        var pq = new PriorityQueue<BorrowRecord, int>(records.Count());
        foreach (var r in records)
            pq.Enqueue(r, -r.OverdueDays);   // negate for max-heap behaviour

        var sorted = new List<BorrowRecord>(pq.Count);
        while (pq.Count > 0) sorted.Add(pq.Dequeue());

        return Result<IEnumerable<BorrowResponseDto>>.Success(
            sorted.Select(br => ToDto(br, br.Book, br.User)));
    }

    public async Task<Result<FineCalculationDto>> CalculateFineAsync(
        int borrowId, CancellationToken ct = default)
    {
        var record = await _uow.Borrows.GetWithDetailsAsync(borrowId, ct);
        if (record is null) return Result<FineCalculationDto>.NotFound("BorrowRecord");

        return Result<FineCalculationDto>.Success(new FineCalculationDto
        {
            BorrowId    = record.Id,
            BookTitle   = record.Book.Title,
            OverdueDays = record.OverdueDays,
            FinePerDay  = record.Book.Fine,
            TotalFine   = record.OverdueDays * record.Book.Fine
        });
    }

    private static BorrowResponseDto ToDto(BorrowRecord r, Book b, User u) => new()
    {
        Id          = r.Id,
        BookTitle   = b.Title,
        MemberName  = u.FullName,
        BorrowedAt  = r.BorrowedAt,
        DueDate     = r.DueDate,
        ReturnedAt  = r.ReturnedAt,
        Status      = r.Status,
        FineAmount  = r.FineAmount,
        OverdueDays = r.OverdueDays
    };
}
