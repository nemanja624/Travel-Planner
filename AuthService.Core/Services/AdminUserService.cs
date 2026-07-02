using AuthService.Core.Common;
using AuthService.Data;
using AuthService.Data.Mapping;
using Contracts.Auth;
using Contracts.Common;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Core.Services;

public sealed class AdminUserService : IAdminUserService
{
    private readonly AuthDbContext dbContext;

    public AdminUserService(AuthDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<UserDto>> GetUsersAsync(CancellationToken cancellationToken = default)
    {
        var users = await dbContext.Users
            .AsNoTracking()
            .OrderBy(user => user.Email)
            .ToListAsync(cancellationToken);

        return users.Select(user => user.ToDto()).ToList();
    }

    public async Task<ServiceResult<UserDto>> UpdateUserRoleAsync(Guid userId, UserRole role, CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Users
            .FirstOrDefaultAsync(user => user.Id == userId, cancellationToken);

        if (user is null)
        {
            return ServiceResult<UserDto>.Failure("User was not found.");
        }

        user.Role = role;
        user.UpdatedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        return ServiceResult<UserDto>.Success(user.ToDto());
    }

    public async Task<ServiceResult<UserDto>> UpdateUserStatusAsync(Guid userId, bool isActive, CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Users
            .FirstOrDefaultAsync(user => user.Id == userId, cancellationToken);

        if (user is null)
        {
            return ServiceResult<UserDto>.Failure("User was not found.");
        }

        user.IsActive = isActive;
        user.UpdatedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        return ServiceResult<UserDto>.Success(user.ToDto());
    }
}
