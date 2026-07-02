namespace Gateway.Security;

public interface ICurrentUserService
{
    bool TryGetCurrentUser(out CurrentUser user);
}
