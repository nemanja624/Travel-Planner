using AuthService.Data.Models;

namespace AuthService.Core.Security;

public interface IAccessTokenService
{
    AccessTokenResult CreateToken(User user);
}
