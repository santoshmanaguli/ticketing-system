using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SparkleTicketing.Api.Contracts;
using SparkleTicketing.Api.Data;
using SparkleTicketing.Api.Models;

namespace SparkleTicketing.Api.Services;

public sealed class AuthService(
    SparkleTicketingDbContext db,
    PasswordHasher<AppUser> passwordHasher,
    IOptions<JwtOptions> jwtOptions)
{
    public async Task<AuthResponseDto> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        if (await db.Users.AnyAsync(user => user.Email == email, cancellationToken))
        {
            throw new AuthException("An account with this email already exists.");
        }

        var user = new AppUser
        {
            FullName = request.FullName.Trim(),
            Email = email,
            Role = UserRole.SupportAgent,
            IsActive = true
        };

        user.PasswordHash = passwordHasher.HashPassword(user, request.Password);
        db.Users.Add(user);
        await db.SaveChangesAsync(cancellationToken);

        return CreateAuthResponse(user);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await db.Users.SingleOrDefaultAsync(candidate => candidate.Email == email, cancellationToken);

        if (user is null || !user.IsActive)
        {
            throw new AuthException("Invalid email or password.");
        }

        var verification = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (verification == PasswordVerificationResult.Failed)
        {
            throw new AuthException("Invalid email or password.");
        }

        return CreateAuthResponse(user);
    }

    public async Task<AuthUserDto?> GetUserAsync(int userId, CancellationToken cancellationToken)
    {
        var user = await db.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(candidate => candidate.Id == userId && candidate.IsActive, cancellationToken);

        return user is null ? null : ToUserDto(user);
    }

    public AuthResponseDto CreateAuthResponse(AppUser user)
    {
        var options = jwtOptions.Value;
        var expiresAt = DateTime.UtcNow.AddMinutes(options.ExpiryMinutes);
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Role, user.Role.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: options.Issuer,
            audience: options.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        return new AuthResponseDto(
            new JwtSecurityTokenHandler().WriteToken(token),
            expiresAt,
            ToUserDto(user));
    }

    public static string HashPassword(PasswordHasher<AppUser> hasher, AppUser user, string password) =>
        hasher.HashPassword(user, password);

    private static AuthUserDto ToUserDto(AppUser user) =>
        new(user.Id, user.FullName, user.Email, user.Role);
}

public sealed class AuthException(string message) : Exception(message);
