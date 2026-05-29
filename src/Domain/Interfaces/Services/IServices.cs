using LibraryAPI.Domain.Common;
using LibraryAPI.Application.DTOs.Auth;
using LibraryAPI.Application.DTOs.Books;
using LibraryAPI.Application.DTOs.Authors;
using LibraryAPI.Application.DTOs.Members;
using LibraryAPI.Application.DTOs.Borrow;

namespace LibraryAPI.Domain.Interfaces.Services;

public interface IAuthService
{
    Task<Result<AuthResponseDto>> RegisterAsync(RegisterRequestDto dto, CancellationToken ct = default);
    Task<Result<AuthResponseDto>> LoginAsync(LoginRequestDto dto, CancellationToken ct = default);
    Task<Result<AuthResponseDto>> RefreshTokenAsync(string refreshToken, CancellationToken ct = default);
    Task<Result<bool>>            RevokeTokenAsync(string refreshToken, CancellationToken ct = default);
    Task<Result<bool>>            ChangePasswordAsync(int userId, ChangePasswordDto dto, CancellationToken ct = default);
}

public interface IBookService
{
    Task<Result<PagedResult<BookSummaryDto>>> GetAllAsync(BookQueryParameters query, CancellationToken ct = default);
    Task<Result<BookDetailDto>>              GetByIdAsync(int id, CancellationToken ct = default);
    Task<Result<BookDetailDto>>              GetByIsbnAsync(string isbn, CancellationToken ct = default);
    Task<Result<BookDetailDto>>              CreateAsync(CreateBookDto dto, CancellationToken ct = default);
    Task<Result<BookDetailDto>>              UpdateAsync(int id, UpdateBookDto dto, CancellationToken ct = default);
    Task<Result<bool>>                       DeleteAsync(int id, CancellationToken ct = default);
    Task<Result<IEnumerable<BookSummaryDto>>> SearchAsync(string term, CancellationToken ct = default);
}

public interface IAuthorService
{
    Task<Result<PagedResult<AuthorDto>>> GetAllAsync(QueryParameters query, CancellationToken ct = default);
    Task<Result<AuthorDetailDto>>        GetByIdAsync(int id, CancellationToken ct = default);
    Task<Result<AuthorDto>>              CreateAsync(CreateAuthorDto dto, CancellationToken ct = default);
    Task<Result<AuthorDto>>              UpdateAsync(int id, UpdateAuthorDto dto, CancellationToken ct = default);
    Task<Result<bool>>                   DeleteAsync(int id, CancellationToken ct = default);
}

public interface IMemberService
{
    Task<Result<PagedResult<MemberDto>>> GetAllAsync(QueryParameters query, CancellationToken ct = default);
    Task<Result<MemberDetailDto>>        GetByIdAsync(int id, CancellationToken ct = default);
    Task<Result<MemberDto>>              UpdateAsync(int id, UpdateMemberDto dto, CancellationToken ct = default);
    Task<Result<bool>>                   DeactivateAsync(int id, CancellationToken ct = default);
    Task<Result<MemberDetailDto>>        GetProfileAsync(int userId, CancellationToken ct = default);
}

public interface IBorrowService
{
    Task<Result<BorrowResponseDto>>              BorrowBookAsync(int userId, BorrowRequestDto dto, CancellationToken ct = default);
    Task<Result<BorrowResponseDto>>              ReturnBookAsync(int userId, int borrowId, CancellationToken ct = default);
    Task<Result<PagedResult<BorrowResponseDto>>> GetAllBorrowsAsync(QueryParameters query, CancellationToken ct = default);
    Task<Result<IEnumerable<BorrowResponseDto>>> GetUserBorrowsAsync(int userId, CancellationToken ct = default);
    Task<Result<IEnumerable<BorrowResponseDto>>> GetOverdueAsync(CancellationToken ct = default);
    Task<Result<FineCalculationDto>>             CalculateFineAsync(int borrowId, CancellationToken ct = default);
}

public interface IJwtService
{
    string GenerateAccessToken(int userId, string email, string role);
    string GenerateRefreshToken();
    int?   ValidateAndGetUserId(string token);
}
