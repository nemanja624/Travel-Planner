using AuthService.Core.Common;
using AuthService.Core.Security;
using AuthService.Data;
using AuthService.Data.Models;
using Contracts.Auth;
using Contracts.Common;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Core.Services;

public sealed class AuthService : IAuthService
{
    private readonly AuthDbContext dbContext;
    private readonly IPasswordHasher passwordHasher;
    private readonly IAccessTokenService accessTokenService;

    public AuthService(AuthDbContext dbContext, IPasswordHasher passwordHasher, IAccessTokenService accessTokenService)
    {
        this.dbContext = dbContext;
        this.passwordHasher = passwordHasher;
        this.accessTokenService = accessTokenService;
    }

    public async Task<ServiceResult<AuthResponse>> RegisterAsync(RegisterUserRequest request, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = NormalizeEmail(request.Email);
        var emailExists = await dbContext.Users
            .AnyAsync(user => user.NormalizedEmail == normalizedEmail, cancellationToken);

        if (emailExists)
        {
            return ServiceResult<AuthResponse>.Failure("User with this email already exists.");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Email = request.Email.Trim(),
            NormalizedEmail = normalizedEmail,
            PasswordHash = passwordHasher.HashPassword(request.Password),
            Role = UserRole.User,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        await dbContext.Users.AddAsync(user, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ServiceResult<AuthResponse>.Success(CreateAuthResponse(user));
    }

    public async Task<ServiceResult<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = NormalizeEmail(request.Email);
        var user = await dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(user => user.NormalizedEmail == normalizedEmail, cancellationToken);

        if (user is null || !user.IsActive || !passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            return ServiceResult<AuthResponse>.Failure("Invalid email or password.");
        }

        return ServiceResult<AuthResponse>.Success(CreateAuthResponse(user));
    }

    private AuthResponse CreateAuthResponse(User user)
    {
        var token = accessTokenService.CreateToken(user);

        return new AuthResponse(
            user.Id,
            user.Name,
            user.Email,
            user.Role,
            token.Token,
            token.ExpiresAtUtc);
    }

    private static string NormalizeEmail(string email)
    {
        return email.Trim().ToUpperInvariant();
    }
}
