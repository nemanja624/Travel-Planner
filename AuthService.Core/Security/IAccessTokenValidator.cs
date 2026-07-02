namespace AuthService.Core.Security;

public interface IAccessTokenValidator
{
    TokenValidationResult ValidateToken(string token);
}
