using Contracts.Common;

namespace Gateway.Security;

public sealed class RoleGuardService : IRoleGuardService
{
    private readonly ICurrentUserService currentUserService;

    public RoleGuardService(ICurrentUserService currentUserService)
    {
        this.currentUserService = currentUserService;
    }

    public bool TryRequireRole(UserRole requiredRole, out CurrentUser user)
    {
        user = default!;

        if (!currentUserService.TryGetCurrentUser(out var currentUser))
        {
            return false;
        }

        if (currentUser.Role != requiredRole)
        {
            return false;
        }

        user = currentUser;
        return true;
    }
}
