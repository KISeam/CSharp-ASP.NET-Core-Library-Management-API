using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LibraryAPI.Domain.Common;
using LibraryAPI.Domain.Interfaces.Services;
using LibraryAPI.Application.DTOs.Auth;
using LibraryAPI.Application.DTOs.Books;
using LibraryAPI.Application.DTOs.Authors;
using LibraryAPI.Application.DTOs.Members;
using LibraryAPI.Application.DTOs.Borrow;

namespace LibraryAPI.API.Controllers;

// ─────────────────────────────────────────────────────────────
// BASE CONTROLLER — shared helpers for all controllers
// ─────────────────────────────────────────────────────────────
[ApiController]
[Produces("application/json")]
public abstract class BaseController : ControllerBase
{
    /// <summary>Turns a Result<T> into the correct HTTP response.</summary>
    protected IActionResult FromResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
        {
            return result.StatusCode == 201
                ? StatusCode(201, ApiResponse<T>.Ok(result.Value!))
                : Ok(ApiResponse<T>.Ok(result.Value!));
        }

        return result.StatusCode switch
        {
            400 => BadRequest(ApiResponse<T>.Fail(result.Error)),
            401 => Unauthorized(ApiResponse<T>.Fail(result.Error)),
            403 => StatusCode(403, ApiResponse<T>.Fail(result.Error)),
            404 => NotFound(ApiResponse<T>.Fail(result.Error)),
            409 => Conflict(ApiResponse<T>.Fail(result.Error)),
            422 => UnprocessableEntity(ApiResponse<T>.Fail(result.Error)),
            _   => StatusCode(result.StatusCode, ApiResponse<T>.Fail(result.Error))
        };
    }

    protected int CurrentUserId =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")
            ?? "0");

    protected string CurrentUserRole =>
        User.FindFirstValue(ClaimTypes.Role) ?? "";

    protected bool IsAdminOrLibrarian =>
        CurrentUserRole is "Admin" or "Librarian";
}

/// <summary>Consistent API envelope — every response has the same shape.</summary>
public record ApiResponse<T>
{
    public bool      Success   { get; init; }
    public T?        Data      { get; init; }
    public string?   Error     { get; init; }
    public DateTime  Timestamp { get; init; } = DateTime.UtcNow;

    public static ApiResponse<T> Ok(T data)      => new() { Success = true,  Data  = data };
    public static ApiResponse<T> Fail(string err) => new() { Success = false, Error = err  };
}

// ─────────────────────────────────────────────────────────────
// AUTH CONTROLLER  — /api/auth
// ─────────────────────────────────────────────────────────────
[Route("api/auth")]
public class AuthController : BaseController
{
    private readonly IAuthService _auth;
    public AuthController(IAuthService auth) => _auth = auth;

    /// <summary>Register a new member account.</summary>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequestDto dto, CancellationToken ct)
        => FromResult(await _auth.RegisterAsync(dto, ct));

    /// <summary>Login and receive access + refresh tokens.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequestDto dto, CancellationToken ct)
        => FromResult(await _auth.LoginAsync(dto, ct));

    /// <summary>Exchange an expired access token using a valid refresh token.</summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh(
        [FromBody] RefreshTokenRequestDto dto, CancellationToken ct)
        => FromResult(await _auth.RefreshTokenAsync(dto.RefreshToken, ct));

    /// <summary>Revoke a refresh token (logout).</summary>
    [HttpPost("revoke")]
    [Authorize]
    public async Task<IActionResult> Revoke(
        [FromBody] RefreshTokenRequestDto dto, CancellationToken ct)
        => FromResult(await _auth.RevokeTokenAsync(dto.RefreshToken, ct));

    /// <summary>Change own password.</summary>
    [HttpPut("password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangePasswordDto dto, CancellationToken ct)
        => FromResult(await _auth.ChangePasswordAsync(CurrentUserId, dto, ct));
}

// ─────────────────────────────────────────────────────────────
// BOOKS CONTROLLER  — /api/books
// ─────────────────────────────────────────────────────────────
[Route("api/books")]
public class BooksController : BaseController
{
    private readonly IBookService _books;
    public BooksController(IBookService books) => _books = books;

    /// <summary>Get paginated book list. Supports search, genre filter, sort.</summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll(
        [FromQuery] BookQueryParameters query, CancellationToken ct)
        => FromResult(await _books.GetAllAsync(query, ct));

    /// <summary>Get book details by ID.</summary>
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
        => FromResult(await _books.GetByIdAsync(id, ct));

    /// <summary>Look up a book by ISBN-13.</summary>
    [HttpGet("isbn/{isbn}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetByIsbn(string isbn, CancellationToken ct)
        => FromResult(await _books.GetByIsbnAsync(isbn, ct));

    /// <summary>Full-text search by title, author name, or ISBN.</summary>
    [HttpGet("search")]
    [AllowAnonymous]
    public async Task<IActionResult> Search([FromQuery] string term, CancellationToken ct)
        => FromResult(await _books.SearchAsync(term, ct));

    /// <summary>Add a new book. Requires Admin or Librarian role.</summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Librarian")]
    public async Task<IActionResult> Create([FromBody] CreateBookDto dto, CancellationToken ct)
        => FromResult(await _books.CreateAsync(dto, ct));

    /// <summary>Update book metadata. Requires Admin or Librarian role.</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Librarian")]
    public async Task<IActionResult> Update(
        int id, [FromBody] UpdateBookDto dto, CancellationToken ct)
        => FromResult(await _books.UpdateAsync(id, dto, ct));

    /// <summary>Soft-delete a book. Admin only.</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
        => FromResult(await _books.DeleteAsync(id, ct));
}

// ─────────────────────────────────────────────────────────────
// AUTHORS CONTROLLER  — /api/authors
// ─────────────────────────────────────────────────────────────
[Route("api/authors")]
public class AuthorsController : BaseController
{
    private readonly IAuthorService _authors;
    public AuthorsController(IAuthorService authors) => _authors = authors;

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll([FromQuery] QueryParameters q, CancellationToken ct)
        => FromResult(await _authors.GetAllAsync(q, ct));

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
        => FromResult(await _authors.GetByIdAsync(id, ct));

    [HttpPost]
    [Authorize(Roles = "Admin,Librarian")]
    public async Task<IActionResult> Create([FromBody] CreateAuthorDto dto, CancellationToken ct)
        => FromResult(await _authors.CreateAsync(dto, ct));

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Librarian")]
    public async Task<IActionResult> Update(
        int id, [FromBody] UpdateAuthorDto dto, CancellationToken ct)
        => FromResult(await _authors.UpdateAsync(id, dto, ct));

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
        => FromResult(await _authors.DeleteAsync(id, ct));
}

// ─────────────────────────────────────────────────────────────
// MEMBERS CONTROLLER  — /api/members
// ─────────────────────────────────────────────────────────────
[Route("api/members")]
public class MembersController : BaseController
{
    private readonly IMemberService _members;
    public MembersController(IMemberService members) => _members = members;

    /// <summary>Admin/Librarian: list all members with pagination.</summary>
    [HttpGet]
    [Authorize(Roles = "Admin,Librarian")]
    public async Task<IActionResult> GetAll([FromQuery] QueryParameters q, CancellationToken ct)
        => FromResult(await _members.GetAllAsync(q, ct));

    /// <summary>Admin/Librarian: view any member by ID.</summary>
    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin,Librarian")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
        => FromResult(await _members.GetByIdAsync(id, ct));

    /// <summary>Any authenticated user can view their own profile.</summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetProfile(CancellationToken ct)
        => FromResult(await _members.GetProfileAsync(CurrentUserId, ct));

    /// <summary>Update own profile. Admin can update any member.</summary>
    [HttpPut("{id:int}")]
    [Authorize]
    public async Task<IActionResult> Update(
        int id, [FromBody] UpdateMemberDto dto, CancellationToken ct)
    {
        // Members can only update themselves; Admin can update anyone
        if (CurrentUserRole != "Admin" && CurrentUserId != id)
            return StatusCode(403, ApiResponse<bool>.Fail("You can only update your own profile."));

        return FromResult(await _members.UpdateAsync(id, dto, ct));
    }

    /// <summary>Admin only: deactivate a member account.</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Deactivate(int id, CancellationToken ct)
        => FromResult(await _members.DeactivateAsync(id, ct));
}

// ─────────────────────────────────────────────────────────────
// BORROW CONTROLLER  — /api/borrows
// ─────────────────────────────────────────────────────────────
[Route("api/borrows")]
[Authorize]
public class BorrowsController : BaseController
{
    private readonly IBorrowService _borrow;
    public BorrowsController(IBorrowService borrow) => _borrow = borrow;

    /// <summary>Admin/Librarian: view all active borrow records.</summary>
    [HttpGet]
    [Authorize(Roles = "Admin,Librarian")]
    public async Task<IActionResult> GetAll([FromQuery] QueryParameters q, CancellationToken ct)
        => FromResult(await _borrow.GetAllBorrowsAsync(q, ct));

    /// <summary>Admin/Librarian: list all overdue borrows, highest overdue first.</summary>
    [HttpGet("overdue")]
    [Authorize(Roles = "Admin,Librarian")]
    public async Task<IActionResult> GetOverdue(CancellationToken ct)
        => FromResult(await _borrow.GetOverdueAsync(ct));

    /// <summary>Any member can view their own borrow history.</summary>
    [HttpGet("my")]
    public async Task<IActionResult> GetMyBorrows(CancellationToken ct)
        => FromResult(await _borrow.GetUserBorrowsAsync(CurrentUserId, ct));

    /// <summary>Borrow a book.</summary>
    [HttpPost]
    public async Task<IActionResult> Borrow([FromBody] BorrowRequestDto dto, CancellationToken ct)
        => FromResult(await _borrow.BorrowBookAsync(CurrentUserId, dto, ct));

    /// <summary>Return a borrowed book.</summary>
    [HttpPut("{borrowId:int}/return")]
    public async Task<IActionResult> Return(int borrowId, CancellationToken ct)
        => FromResult(await _borrow.ReturnBookAsync(CurrentUserId, borrowId, ct));

    /// <summary>Calculate fine for a specific borrow record.</summary>
    [HttpGet("{borrowId:int}/fine")]
    public async Task<IActionResult> CalculateFine(int borrowId, CancellationToken ct)
        => FromResult(await _borrow.CalculateFineAsync(borrowId, ct));
}
