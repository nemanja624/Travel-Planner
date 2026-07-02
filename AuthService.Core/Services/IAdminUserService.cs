using AuthService.Core.Common;
using Contracts.Auth;
using Contracts.Common;

namespace AuthService.Core.Services;

public interface IAdminUserService
{
    Task<IReadOnlyCollection<UserDto>> GetUsersAsync(CancellationToken cancellationToken = default);

    Task<ServiceResult<UserDto>> UpdateUserRoleAsync(Guid userId, UserRole role, CancellationToken cancellationToken = default);

    Task<ServiceResult<UserDto>> UpdateUserStatusAsync(Guid userId, bool isActive, CancellationToken cancellationToken = default);
}
