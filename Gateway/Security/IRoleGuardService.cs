using Contracts.Common;

namespace Gateway.Security;

public interface IRoleGuardService
{
    bool TryRequireRole(UserRole requiredRole, out CurrentUser user);
}
