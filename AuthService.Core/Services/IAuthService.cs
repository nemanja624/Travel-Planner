using AuthService.Core.Common;
using Contracts.Auth;

namespace AuthService.Core.Services;

public interface IAuthService
{
    Task<ServiceResult<AuthResponse>> RegisterAsync(RegisterUserRequest request, CancellationToken cancellationToken = default);

    Task<ServiceResult<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
}
