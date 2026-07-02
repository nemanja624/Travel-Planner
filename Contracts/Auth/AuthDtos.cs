using System.ComponentModel.DataAnnotations;
using Contracts.Common;

namespace Contracts.Auth;

public sealed record RegisterUserRequest(
    [Required, MaxLength(100)] string Name,
    [Required, EmailAddress, MaxLength(256)] string Email,
    [Required, MinLength(8), MaxLength(100)] string Password);

public sealed record LoginRequest(
    [Required, EmailAddress, MaxLength(256)] string Email,
    [Required, MaxLength(100)] string Password);

public sealed record AuthResponse(
    Guid UserId,
    string Name,
    string Email,
    UserRole Role,
    string AccessToken,
    DateTime ExpiresAtUtc);

public sealed record UserDto(
    Guid Id,
    string Name,
    string Email,
    UserRole Role,
    bool IsActive,
    DateTime CreatedAtUtc);

public sealed record UpdateUserRoleRequest(
    UserRole Role);

public sealed record UpdateUserStatusRequest(
    bool IsActive);
