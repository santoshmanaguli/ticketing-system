using System.ComponentModel.DataAnnotations;
using SparkleTicketing.Api.Models;

namespace SparkleTicketing.Api.Contracts;

public sealed class RegisterRequest
{
    [Required, MinLength(2), MaxLength(120)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(160)]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(8), MaxLength(128)]
    public string Password { get; set; } = string.Empty;
}

public sealed class LoginRequest
{
    [Required, EmailAddress, MaxLength(160)]
    public string Email { get; set; } = string.Empty;

    [Required, MaxLength(128)]
    public string Password { get; set; } = string.Empty;
}

public sealed record AuthUserDto(int Id, string FullName, string Email, UserRole Role);

public sealed record AuthResponseDto(string Token, DateTime ExpiresAt, AuthUserDto User);
