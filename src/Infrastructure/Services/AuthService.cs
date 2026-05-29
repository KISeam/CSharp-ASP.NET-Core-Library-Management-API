using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using BCrypt.Net;
using LibraryAPI.Domain.Common;
using LibraryAPI.Domain.Entities;
using LibraryAPI.Domain.Interfaces.Repositories;
using LibraryAPI.Domain.Interfaces.Services;
using LibraryAPI.Application.DTOs.Auth;

namespace LibraryAPI.Infrastructure.Services;

// ─────────────────────────────────────────────────────────────
// JWT SERVICE
// ─────────────────────────────────────────────────────────────
public class JwtService : IJwtService
{
    private readonly IConfiguration _config;
    private readonly string _secret;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int    _accessMinutes;

    public JwtService(IConfiguration config)
    {
        _config        = config;
        _secret        = config["Jwt:Secret"]   ?? throw new InvalidOperationException("Jwt:Secret missing");
        _issuer        = config["Jwt:Issuer"]   ?? "LibraryAPI";
        _audience      = config["Jwt:Audience"] ?? "LibraryAPI";
        _accessMinutes = int.Parse(config["Jwt:AccessTokenMinutes"] ?? "60");
    }

    /// <summary>Creates a signed JWT containing userId, email, and role claims.</summary>
    public string GenerateAccessToken(int userId, string email, string role)
    {
        var key         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub,   userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(ClaimTypes.Role,               role),
            new Claim(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat,
                DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64)
        };

        var token = new JwtSecurityToken(
            issuer:             _issuer,
            audience:           _audience,
            claims:             claims,
            expires:            DateTime.UtcNow.AddMinutes(_accessMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>Cryptographically random 64-byte refresh token — not a JWT.</summary>
    public string GenerateRefreshToken()
    {
        var bytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }

    /// <summary>Validates token signature & expiry; returns userId or null.</summary>
    public int? ValidateAndGetUserId(string token)
    {
        try
        {
            var handler    = new JwtSecurityTokenHandler();
            var key        = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
            var parameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey         = key,
                ValidateIssuer           = true,
                ValidIssuer              = _issuer,
                ValidateAudience         = true,
                ValidAudience            = _audience,
                ValidateLifetime         = true,
                ClockSkew                = TimeSpan.Zero
            };

            var principal = handler.ValidateToken(token, parameters, out _);
            var sub       = principal.FindFirstValue(JwtRegisteredClaimNames.Sub);
            return int.TryParse(sub, out var id) ? id : null;
        }
        catch { return null; }
    }
}

// ─────────────────────────────────────────────────────────────
// AUTH SERVICE
// ─────────────────────────────────────────────────────────────
public class AuthService : IAuthService
{
    private readonly IUnitOfWork _uow;
    private readonly IJwtService _jwt;
    private readonly IConfiguration _config;
    private readonly int _refreshDays;

    public AuthService(IUnitOfWork uow, IJwtService jwt, IConfiguration config)
    {
        _uow         = uow;
        _jwt         = jwt;
        _config      = config;
        _refreshDays = int.Parse(config["Jwt:RefreshTokenDays"] ?? "7");
    }

    public async Task<Result<AuthResponseDto>> RegisterAsync(
        RegisterRequestDto dto, CancellationToken ct = default)
    {
        // Guard: duplicate email
        if (await _uow.Users.EmailExistsAsync(dto.Email.ToLower(), ct))
            return Result<AuthResponseDto>.Conflict("Email is already registered.");

        var user = new User
        {
            FirstName    = dto.FirstName.Trim(),
            LastName     = dto.LastName.Trim(),
            Email        = dto.Email.ToLower().Trim(),
            PhoneNumber  = dto.PhoneNumber ?? "",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password, workFactor: 12),
            Role         = Domain.Enums.UserRole.Member
        };

        await _uow.Users.AddAsync(user, ct);
        await _uow.SaveChangesAsync(ct);

        return await IssueTokenPairAsync(user, ct);
    }

    public async Task<Result<AuthResponseDto>> LoginAsync(
        LoginRequestDto dto, CancellationToken ct = default)
    {
        var user = await _uow.Users.GetByEmailAsync(dto.Email.ToLower(), ct);

        // Use constant-time comparison to prevent timing attacks
        if (user is null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return Result<AuthResponseDto>.Unauthorized("Invalid email or password.");

        if (!user.IsActive)
            return Result<AuthResponseDto>.Forbidden("Account is deactivated.");

        user.LastLoginAt = DateTime.UtcNow;
        _uow.Users.Update(user);
        await _uow.SaveChangesAsync(ct);

        return await IssueTokenPairAsync(user, ct);
    }

    public async Task<Result<AuthResponseDto>> RefreshTokenAsync(
        string refreshToken, CancellationToken ct = default)
    {
        var stored = await _uow.RefreshTokens.GetByTokenAsync(refreshToken, ct);

        if (stored is null || stored.IsRevoked || stored.ExpiresAt < DateTime.UtcNow)
            return Result<AuthResponseDto>.Unauthorized("Refresh token is invalid or expired.");

        // Rotate: revoke old, issue new
        stored.IsRevoked = true;
        _uow.RefreshTokens.Update(stored);

        return await IssueTokenPairAsync(stored.User, ct);
    }

    public async Task<Result<bool>> RevokeTokenAsync(
        string refreshToken, CancellationToken ct = default)
    {
        var stored = await _uow.RefreshTokens.GetByTokenAsync(refreshToken, ct);
        if (stored is null) return Result<bool>.NotFound("RefreshToken");

        stored.IsRevoked = true;
        _uow.RefreshTokens.Update(stored);
        await _uow.SaveChangesAsync(ct);
        return Result<bool>.Success(true);
    }

    public async Task<Result<bool>> ChangePasswordAsync(
        int userId, ChangePasswordDto dto, CancellationToken ct = default)
    {
        var user = await _uow.Users.GetByIdAsync(userId, ct);
        if (user is null) return Result<bool>.NotFound("User");

        if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
            return Result<bool>.Failure("Current password is incorrect.", 400);

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword, workFactor: 12);
        _uow.Users.Update(user);

        // Revoke all refresh tokens on password change (security best practice)
        await _uow.RefreshTokens.RevokeAllForUserAsync(userId, ct);
        await _uow.SaveChangesAsync(ct);

        return Result<bool>.Success(true);
    }

    // ── Private helpers ───────────────────────────────────────
    private async Task<Result<AuthResponseDto>> IssueTokenPairAsync(
        User user, CancellationToken ct)
    {
        var accessToken  = _jwt.GenerateAccessToken(user.Id, user.Email, user.Role.ToString());
        var refreshToken = _jwt.GenerateRefreshToken();

        var stored = new RefreshToken
        {
            UserId    = user.Id,
            Token     = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(_refreshDays)
        };
        await _uow.RefreshTokens.AddAsync(stored, ct);
        await _uow.SaveChangesAsync(ct);

        return Result<AuthResponseDto>.Success(new AuthResponseDto
        {
            AccessToken  = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt    = DateTime.UtcNow.AddMinutes(
                               int.Parse(_config["Jwt:AccessTokenMinutes"] ?? "60")),
            User = new UserInfoDto
            {
                Id       = user.Id,
                FullName = user.FullName,
                Email    = user.Email,
                Role     = user.Role.ToString()
            }
        });
    }
}
